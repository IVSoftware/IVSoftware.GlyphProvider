# [<](../../README.md)

## How to Copy `icon-basics.ttf` from this NuGet.
___

### Step 1 - Navigate to the NuGet source folder.

This tutorial will use a custom font created on [Fontello](https://fontello.com) that is included as content with this NuGet bundle. If you'd like to make your own custom font, use the folder from the zip archive that you download instead.

<img src="https://github.com/IVSoftware/IVSoftware.Portable.GlyphProvider/blob/master/IVSoftware.Portable.GlyphProvider/README/img/navigate-to-bundle.png" width="600"><br>

_This opens the package in Explorer and the `icon-basics` folder is found inside `content`._

<img src="https://github.com/IVSoftware/IVSoftware.Portable.GlyphProvider/blob/master/IVSoftware.Portable.GlyphProvider/README/img/copy-icon-basics-from-content.png" width="500">

___

### Step 2 - Copy the `icon-basics` Folder from the NuGet bundle.

In your project, create the `Resources\Fonts` directory if it doesn't already exist (e.g. a fresh WinForms project). Locate and open the **content** folder in the NuGet and copy the entire **icon-basics** folder to the `Resources\Fonts` directory. 


<img src="https://github.com/IVSoftware/IVSoftware.Portable.GlyphProvider/blob/master/IVSoftware.Portable.GlyphProvider/README/img/embed-the-resource.png" width="600"><br>

### Step 3 - Set the Build Property of `config.json` to **Embedded Resource** as shown above.
___

## Platform Specific Quick Start Examples

