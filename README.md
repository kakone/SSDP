# SSDP.Portable
.NET Standard implementation of Simple Service Discovery Protocol (with AV transport service control point sample).<br />

## Usage
### Devices search
```C#
var devices = await new Ssdp().SearchUPnPDevicesAsync("MediaRenderer");
```
### UPnP AV transport service control point
```C#
var controlPoint = new AVTransportControlPoint(new Ssdp());

// Gets the list of the media renderers
var mediaRenderers = await controlPoint.GetMediaRenderersAsync();

// Play the Big Buck Bunny video to a media renderer
await controlPoint.PlayAsync(mediaRenderers.First(), "http://download.blender.org/peach/bigbuckbunny_movies/big_buck_bunny_480p_surround-fix.avi");
```

## Download
[![NuGet Status](http://img.shields.io/nuget/v/SSDP.Portable.svg?style=flat)](https://www.nuget.org/packages/SSDP.Portable)
