# How to publish Olive Nuget Package

Olive nuget package is published automatically using Github Actions. For this automatic publish to happen we need to take these few steps:
- Navigate to ```Olive.csproj``` and update the version inside.
 ```<Version>x.y.z.m</Version> ==> <Version>x`.y`.z`.m`</Version>```
- Create a new Tag here https://github.com/Geeksltd/Olive/tags and set the tag to vx`.y`.z`.m` .
- Wait for it to be published.

