
<p align="center">
  <a href="https://geeksltd.github.io/Olive/">
    <img alt="Olive" src="./docs/_media/Olive.png">
  </a>
</p>

<p align="center">
  Olive: More productive .NET development.
</p>
<p align="center" > <a href="https://geeksltd.github.io/Olive/"> <strong> See documentation </strong> </a></p>

<p align="center">
  <a href="https://www.nuget.org/packages/Olive/"><img alt="Olive" src="https://img.shields.io/nuget/v/Olive.svg"></a>
  

# Olive

Olive is a framework built on top of .NET for more productive cross-platform software development in .NET solutions. It provides a set of productivity tools to make .NET development easier, cleaner and more expressive. 

As a .NET Standard 2.0 library it's compatible with almost any .NET stack.
Check out the Olive documentation [HERE](geeksltd.github.com/Olive)

## Contributing

As this solution is an opensource project, so contributions are welcome! Just fork the repo, do your changes then make a merge request.
We'll review your code ASAP and we will do the merge if everything was just fine. If there was any kind of issues, please post it in the [issues](https://github.com/Geeksltd/Olive/issues) section.

### Build and running the code

Olive projects might have dependencies to each other. You might need to build other projects recursively to run a certain project. All projects have dependencies to **Olive** project; So you need to build this one first.
Also you can run **BuildAll.bat**, which is located in the root of the project, to build all of the Olive projects.

### publish Olive Nuget Package

Olive nuget package is published automatically using Github Actions. For this automatic publish to happen we need to take these few steps:
- Navigate to ```Olive.csproj``` and update the version inside.
 ```<Version>x.y.z.m</Version> ==> <Version>x`.y`.z`.m`</Version>```
- Create a new Tag here https://github.com/Geeksltd/Olive/tags and set the tag to ```vx`.y`.z`.m` ```.
- Wait for it to be published.


### Authors

This project is maintained and supported by Geeks Ltd.

See also the list of [contributors](https://github.com/Geeksltd/Olive/contributors) who participated in this project.

## License

This project is available under the GPL v3 license. See [License.md](License.md) for more information.
