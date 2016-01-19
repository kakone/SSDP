# SSDP.Portable
.NET implementation of Simple Service Discovery Protocol (with AV transport service control point sample)<br />
*Portable Class Library* for Windows Phone and Windows Store Apps (8.1/UWP), Desktop Apps (.Net FX 4.5) and Xamarin Android Apps.

##Usage
### Devices search
```C#
var devices = await new Ssdp().SearchUPnPDevices("MediaRenderer");
```
### UPnP AV transport service control point
```C#
var controlPoint = new AVTransportControlPoint(new Ssdp());

// Gets the list of the media renderers
var mediaRenderers = await controlPoint.GetMediaRenderers();

// Play the Big Buck Bunny video to a media renderer
await controlPoint.Play(mediaRenderers.First(), "http://download.blender.org/peach/bigbuckbunny_movies/big_buck_bunny_480p_surround-fix.avi");
```

##Download
[![NuGet Status](http://img.shields.io/nuget/v/SSDP.Portable.svg?style=flat)](https://www.nuget.org/packages/SSDP.Portable)
[![Nuget](https://img.shields.io/nuget/dt/SSDP.Portable.svg)](https://www.nuget.org/packages/SSDP.Portable)
