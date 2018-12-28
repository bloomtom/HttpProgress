# HttpProgress
>A set of extension methods for `HttpClient` which adds progress reporting.

HttpClient doesn't natively support progress reporting? Yes, it's true. You're expected to make your own `HttpContent` which overrides `SerializeToStreamAsync` for `PutAsync` and `PostAsync`, and you're expected to extend `GetAsync` with progress reporting in the stream copy logic. Well guess what? I've done that for you. 

## Nuget Packages

Package Name | Target Framework | Version
---|---|---
[HttpProgress](https://www.nuget.org/packages/bloomtom.HttpProgress) | .NET Standard 2.0 | ![NuGet](https://img.shields.io/nuget/v/bloomtom.HttpProgress.svg)


## Usage
Add HttpProgress to your usings.
```csharp
using HttpProgress;
```
Use the `HttpClient` extensions.
```csharp
// Make your progress event.
var progress = new Progress<ICopyProgress>(x => // Please see "Notes on IProgress<T>"
{
    // This is your progress event!
    // It will fire on every buffer fill so don't do anything expensive.
    // Writing to the console IS expensive, so don't do the following in practice...
    Console.WriteLine(x.PercentComplete.ToString("P"));
});

// Post
using (var fileStream = System.IO.File.OpenRead("MyFile.txt"))
{
    var result = await client.PostAsync("https://mysite.com/something", fileStream, progress);
}

// Get
using (var downloadStream = new MemoryStream())
{
    var response = await client.GetAsync("https://mysite.com/something", downloadStream, progress);
}
```
Woah, that was easy. But what if you want more than just percent complete? Well the progress event actually gives you all of the following in `ICopyProgress`:
```csharp
public interface ICopyProgress
{
	/// <summary>
	/// The instantaneous data transfer rate.
	/// </summary>
	int BytesPerSecond { get; }
	/// <summary>
	/// The total number of bytes transfered so far.
	/// </summary>
	long BytesTransfered { get; }
	/// <summary>
	/// The total number of bytes expected to be copied.
	/// </summary>
	long ExpectedBytes { get; }
	/// <summary>
	/// The percentage complete as a value 0-1.
	/// </summary>
	double PercentComplete { get; }
	/// <summary>
	/// The total time elapsed so far.
	/// </summary>
	TimeSpan TransferTime { get; }
}
```

## Notes on `IProgress<T>`

#### Concrete Implementation

The type [`Progress<T>`](https://docs.microsoft.com/en-us/dotnet/api/system.progress-1) is provided by the framework for use cases where the progress changed event should be processed through your application's synchronization context. This is useful for desktop UI applications where modifying controls from the progress event needs to be done on the event loop thread. For console applications, which have no intelligent synchronization context, the event will end up on the thread pool and _will likely be executed out of order_. If this is undesirable, you can use your own implementation of `IProgress`. Doing this is very easy, and an example is provided in the unit test project class [`NaiveProgress<T>`](HttpProgressTests/NaiveProgress.cs).

#### Performance Considerations

The action you give for progress reporting will fire on every buffer cycle. This can happen _many_ times! With a 16kB buffer transfering a 10MB file will cause 640 events to be fired.

If you need to do time consuming operations, consider rate limiting them. A simple way to do this is to only fire your expensive operation when `TransferTime` or `PercentComplete` crosses specific thresholds. A more complex but "prettier" solution is to buffer `ICopyProgress` and have a threaded reader to refresh your UI on a timer.