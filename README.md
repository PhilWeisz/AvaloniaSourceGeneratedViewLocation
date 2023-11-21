For my brilliant friend Steve.

AOT-Safe, Source-Generated, ViewLocation for Avalonia
===========================
      
1) Assembly-Setup
1.1) The Source-Gen assembly is netstandard2.0 and requires: 

 ==========================
   <PackageReference Include="Microsoft.CodeAnalysis" Version="3.8.0" />
   <PackageReference Include="Microsoft.CodeAnalysis.CSharp" Version="3.8.0" />
   <PackageReference Include="Microsoft.CSharp" Version="4.7.0" />
 =========================

  [ It's only v3.8.0 because Avalonia requires exactly that version. ] 

1.2) Keep in mind there can be problems when trying to change the roslyn-
     assemblies' versions due to the Avaloniaan dependency.

++++++++++++++++++++++++++

2) Usage
2.1) Set the import-usings via the `AddNamesspaceUsings()`-method [marked `A)`]
2.2) Set the namespace-name via the field `s_Setting_Namespace` [marked`B)`] 
2.3) In `App.xaml.cs` at invocationstart of `OnFrameworkInitialized()` init the ViewLocator
	 and call the `ViewLocator.PopulateViewModelViewMappings()`-method [marked `C)`]
2.4) ViewLocator-class needs to be `partial`
2.5) In the class that references the SourceGen, modify the reference, adding
     `OutputItemType="Analyzer" ReferenceOutputAssembly="false"` to the end of the Project-Ref.
 
 ==========================
  <ProjectReference Include="..\SourceGenerator.ViewLocator\SourceGenerator.ViewLocator.csproj"
	  OutputItemType="Analyzer" ReferenceOutputAssembly="false" />  
 ==========================

 2.6) Make sure the ViewLocator.cs class provides the same functionality like (or is) the ViewLocator.cs 
      in this project. It needs the Locator-Dictionary and has to use the factories.


 3)   Testing / Debugging
      I'd like to see what the Source-Gen generates...   
      
      In the .csproj that references the Source-Generator add these settings to the main Propertygroup:

 =========================
  <AvaloniaUseCompiledBindingsByDefault>true</AvaloniaUseCompiledBindingsByDefault>
  <CompilerGeneratedFilesOutputPath>$(ProjectDir)GeneratedFiles</CompilerGeneratedFilesOutputPath>
 =========================

   In the Main-Solution-Directory will spawn a `GeneratedFiles`-dir which hosts the generated sources. 
   In order to be able to run the project again, you will have to delete these files quite certainly.
   [since Avalonia's source-generated files will also spawn there, and create duplicate Type-Errors]
