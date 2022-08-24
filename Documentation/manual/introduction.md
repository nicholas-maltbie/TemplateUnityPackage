# Template Unity Package Introduction

Introduction to Nick Maltbie's Template Unity Package.

## Usage

This project is intended to be used as a base for creating
new unity packages. The goal for this is to make creating and
sharing unity packages via git and npm easier for new projects.
I'm creating this project so I have a template when I want
to create a new package such as [OpenKCC](https://github.com/nicholas-maltbie/OpenKCC)
or [ScreenManager](https://github.com/nicholas-maltbie/ScreenManager).

This project contains a template setup for a unity package.

* Example package with example script.
* Automated testing validation in PlayMode and EditMode.
* Template `package.json` configuration.

This also includes the scripts and actions to upload
the build to an npm repo such as my example npm package
[com.nickmaltbie.openkcc](https://www.npmjs.com/package/com.nickmaltbie.openkcc).
This also includes the scripts to create branches based on tags
and releases of the project in the `setup-package.sh` file.

## Features

Features of this template package include:

* Rename Script for Unity Assets
* Script to Create Package
* Automated GitHub Actions

### Rename Script

There is a script file at `Assets\Editor\RenameAssets.cs` that
has steps to rename all the files in the project associated
with the template package (like the project name, website, company
name, etc...) as well as regenerate the project asset GUIDs
for unity to avoid asset collisions in multiple uses of
the template.

See the [Rename Script](rename_script.md) for details on how
to use the rename script in the project.

### Script to Create Package

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

### Automated GitHub actions

There are many automated github actions in
the project stored at `.github/actions`

| Workflow | Description |
|----------|-------------|
| `build-verification.yml` | Verify that project can build for a specific platform (default WebGL) properly. |
| `create-package.yml` | Creates a package and attempts to push it to a github branch and npmjs repo. |
| `deploy.yml` | Builds and deploys project to the `gh-pages` branch. |
| `format.yml` | Verifies formatting for project with `markdownlint`, `dotnet format`, and `docfx` |
| `tests-validation.yml` | Runs test validation on the project in EditMode and PlayMode |

Please look at the workflow files for further details and
documentation on the github repo.

There are some global configuration variables all fo the workflows
in the file `.github/variables/projectconfig.env`.

And there are a few shared workflow scripts under `.github/actions` folder
including:

| Action | Description |
|--------|-------------|
| `git-lfs-cache` | Loads files from `git-lfs` using a cache from the list of lfs files. |
| `setvars` | Loads variables from the `projectconfig.env` file. |
| `unity-library-cache` | Caches the unity library folders as part of github build. |
