
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

Olive is a framework built on top of .NET for more productive cross-platform software development in .NET solutions. It provides a set of productivity tools to make .NET development easier, cleaner and more expressive. It is created, maintained and supported by Geeks Ltd.

- **Compatibility**: As a .NET Standard 2.0 library it's compatible with almost any .NET stack. 
- **License**: Available under the MIT license. See [License.md](License.md) for more information.
- **Contributions welcome**: Just fork the repo, do your changes then make a merge request.
- **Found an issue?** [report here](https://github.com/Geeksltd/Olive/issues).

Check out the Olive documentation [HERE](geeksltd.github.com/Olive)
  
--- 
# Debugging
  
## How to build
Olive projects have dependencies to each other. You might need to build other projects recursively to run a certain project. All projects have dependencies to **Olive** project; So you need to build this one first. Also you can run **BuildAll.bat**, which is located in the root of the project, to build all of the Olive projects.
  
## Important Tip
You may be  working on a project which has a NuGet dependency to Olive. You have a bug and suspect that the problem may be in Olive, or maybe you just want more diagnostics information. The following is a handy hack:

1. Compile Olive locally, so the source code lines resolve correctly
2. Copy the new DLL and PDB files to your local nuget cache (%UserProfile%\\.nuget\packages)
3. Compile your project again. It will update the DLL from the nuget cache, which is overriden by your locally compiled dll.
4. In Visual Studio, simply press F11 to step into any Olive calls.



