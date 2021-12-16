# How to use:

1. Create a new repository using this as a template (follow the instructions [here](https://docs.github.com/en/free-pro-team@latest/github/creating-cloning-and-archiving-repositories/creating-a-repository-from-a-template)).

2. Clone it somewhere locally - or add is as a submodule to an existing project.

3. Rename the folder Packages/Virsabi.Example to your new library name. eg: Packages/Virsabi.NewLib

4. Edit the `package.json` folder to fit your new library, and set the version to 0.0.0: **Name must be lower case!**

   ![package.json changes.ping](/Resources/package.json%20changes.ping.png)

   

5. Change the path variables in the following to scripts to the renamed folder;

   `.github/workflows/ci.yml`

   ![Changes to .github-workflows-ci.yml](/Resources/Changes%20to%20.github-workflows-ci.yml.png)

   AND		

​	`.releaserc.json`

​	![Changes to .releaseRC.json](/Resources/Changes%20to%20.releaseRC.json.png)

**IMPOTANT!** - these paths are case sensitive!

**DOUBLE IMPORTANT!** - github does not allow pushing to the .github folder by using normal credentials, and Sourcetree doesn't automatically setup ssh keys, so its easiest to edit the ci.yml file directly on github in the browser. 

The error you'll most likely get if trying to push ci.yml without ssh keys:
 ![Changes to .github-workflows-ci.yml](/Resources/sourceTree%20error%20-%20use%20git%20github.com%20virsabi%20Virsabi.Example.git%20instead%20%2B%20generate%20keys.png)

4. Commit with a message starting with`fix:`/`feat:` to trigger an auto release and creation of the upm branch.
5. Check if the action completes without errors and that a release + upm branch is generated.
6. Make the new upm branch default/head of the repository if planning to import using git in Unity Package Manager, since they currently use the default branch, and you only want to import what's inside `Packages/Virsabi.NewLib/*`  
7. Delete the `resources` folder (containing these images) and rewrite this readme.



# Developing the new library

Follow the general guidelines in https://github.com/Virsabi/Virsabi.Core/tree/main.
You can only edit and push changes to a library added to unity from disk (not when adding from github as upm package) - therefore add the package as a submodule in the root of your unity project;

- Assets/*
- Packages/*
- Submodules/
  - Virsabi.Core
  - NewLib

and then in unity add the package from disk using the package manager.

This allows the package manager in unity to find the project across computers (when collaborating), and allows each production project to be on different commits/branches of the submodule/library.
