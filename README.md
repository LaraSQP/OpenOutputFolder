# Open Output Folder in ConEmu and Total Commander

[![Build status](https://ci.appveyor.com/api/projects/status/k4tuto4slae2kj0e?svg=true
)](https://ci.appveyor.com/project/LaraSQP/openoutputfolder)
[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](license.txt)

This extension makes it possible to open the output folders of a solution in `ConEmu` and `Total Commander`.

Optionally, the extension can also open the folder of the selected item in the `Solution Explorer` window.


## Setup

Install from the `Open VSIX gallery` via the `Extensions -> Manage Extensions` menu (you might need to add the feed, see [this](http://vsixgallery.com/guide/feed/)) or [download the latest CI build](http://vsixgallery.com/extension/OpenOutputFolder.535e1a7e-26c5-48b9-a140-93ae7291dc98/) as a `VSIX` package and install it manually.

- **Note that** the `releases` tab at `GitHub` will either be empty or have out-of-date releases.

## Getting Started

- Once installed, the `Extensions` menu will show the entry ![off](https://user-images.githubusercontent.com/12540983/85833202-4f4ed800-b7cc-11ea-9d5d-5bece4a79ef4.png) `Open output folder`, as shown below:

![menu](https://user-images.githubusercontent.com/12540983/85833199-4eb64180-b7cc-11ea-8a38-3b595ca7501f.png)

- A quick way to get to **OpenOutputFolder** is to add the extension to the toolbar, as shown in the image below (bottom right below) or to manually assign a shortcut to it via `Options -> Keyboard -> Openoutputfolder`.

![toolbar](https://user-images.githubusercontent.com/12540983/85833205-4fe76e80-b7cc-11ea-97ef-bcecdd02fc69.png)


- **Note that** the first time `ConEmu` or `Total Commander` are invoked, the user will be prompted for the location of the corresponding executable. Were this location to change, the user will be prompted again. Otherwise, **OpenOutputFolder** remembers these locations once first set.

- On running **OpenOutputFolder**, it will find the `selected item` in the `Solution Explorer` window, proceed to list all projects and their configurations in its solution, and highlight the one that corresponds to the `selected item` in the `configurations listbox`, as shown below.

![pop](https://user-images.githubusercontent.com/12540983/85833417-a6ed4380-b7cc-11ea-9c25-dbee27abf1e7.png)


- The default button is `TC`, which will take the selected folder from the `configurations listbox` and open it in a new tab in the `right panel` (per the appropriate checkbox) of `Total Commander` and bring it to the foreground. Thus, in general, pressing 'Enter' upon invoking **OpenOutputFolder** is sufficient.

- If the button clicked on is `ConEmu`, the selected folder from the listbox will be opened in a new tab in the `ConEmu`.

- In both cases, running instances of `ConEmu/Total Commander` will be used, if available.

- If the checkbox `Active item` is checked, `ConEmu/Total Commander` will open the folder containing the selected item in the `Solution Explorer` window instead of the selection in the `configurations listbox`. Note that `Active item` will "stick" and, if checked, will pop up checked next time **OpenOutputFolder** in run.

- Double-clicking on a configuration opens it in `Total Commander` without further ado, ignoring the state of the `Active Item` checkbox.

- If by any chance there are no items selected in the `Solution Explorer` window, **OpenOutputFolder** will try to use the currently active document in the IDE. Failing that, **OpenOutputFolder** will give up quietly and not even open.

## Limitations

It is safe to assume that **OpenOutputFolder** has some limitations since it has only been tested with `c# solutions` and on just a few of machines.
  
