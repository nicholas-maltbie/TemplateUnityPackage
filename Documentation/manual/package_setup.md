# Package Setup

Custom packages can be used to easily share code and assets between different
unity projects. However, creating a package in unity can sometimes take
a bit of work to get right. The instructions on how to create custom
packages is provided on Unity's documentation:
[Creating Custom Package](https://docs.unity3d.com/2022.1/Documentation/Manual/CustomPackages.html)

This requires quite a bit of work to setup and creating samples can be
a bit difficult, so I created a script to help automate the process.

The script `setup-package.sh` can be used to create a branch
for a given version of the package's source code.

## Usage

Example Usage:

```bash
setup-package.sh $package_path [$tag]
```

Creates a package of the unity files at the `$package_path` folder
and will include the files form `./Assets/Samples` in the export
under the path `Samples~` to follow unity convention.

Will also preserve any `git-lfs` links for files to avoid
duplicating assets in the repo.

Arguments:

* `$package_path` - Required, path to package folder of project,
      Should be something like `Packages/com.companyname.packagename`
* `[$tag]` - Optional, tag version to checkout before building
      package. If provided, will create a new branch with
      the name pattern `release/$tag`

## Samples

Normally, a package samples would be stored in a folder called `Samples~`, this
is because unity will ignore those files in the editor and allow the user
to import them as requested. However, the `~` suffix makes it so that these
samples are difficult to manage within the editor as they are hidden.

To get around this problem, the setup script will copy any samples stored
in the folder `.Assets/Samples` to the folder `Samples~` and preserve
the git history of those files to avoid duplication and increasing the size of
the repo significantly.
