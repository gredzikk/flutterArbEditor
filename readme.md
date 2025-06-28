# WPF Starter Template

A clean and ready-to-use WPF project template for .NET, designed to kickstart your desktop application development. It includes essential features like logging, dependency injection, and automated release packaging.

## Features

- **Dependency Injection**: Pre-configured with `Microsoft.Extensions.DependencyInjection` for building modular and testable applications.
- **File Logging**: A simple yet effective file logger ([`ILogger`](flutterArbEditor/Logger.cs)) is set up to log application events, warnings, and errors. Logs are stored in a `logs` directory.
- **Automatic Version Display**: The application's version number is automatically retrieved from the assembly ([`AssemblyInfo.cs`](flutterArbEditor/AssemblyInfo.cs)) and displayed in the [`MainWindow`](flutterArbEditor/MainWindow.xaml).
- **Automated Release Packaging**: When you build the project in `Release` configuration, a ZIP archive of the application is automatically created in the [`Releases`](Releases/) folder.
- **.NET Template**: Easily create new projects from this template using the `dotnet new` command, with automatic renaming of the project and namespaces.

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) or later.

## Installation

1.  Clone or download this repository.
2.  Open a terminal or command prompt and navigate to the root directory of the `flutterArbEditor` project.
3.  Run the following command to install the project as a new template on your machine:

    ```sh
    dotnet new --install .
    ```

## Usage

Once the template is installed, you can create a new project from it.

1.  Navigate to the directory where you want to create your new project.
2.  Run the following command, replacing `MyNewAwesomeApp` with your desired project name:

    ```sh
    dotnet new wpfstart -n MyNewAwesomeApp
    ```

This will create a new folder named `MyNewAwesomeApp` containing your new WPF project, with the solution, project files, and namespaces all correctly named.

## Configuration

### Application Icon

The application icon can be changed by replacing the [`Assets/icons8-template-64.ico`](flutterArbEditor/Assets/icons8-template-64.ico) file and updating the `<ApplicationIcon>` tag in the [`flutterArbEditor.csproj`](flutterArbEditor/flutterArbEditor.csproj) file.

### Assembly Version

The version number is managed in [`AssemblyInfo.cs`](flutterArbEditor/AssemblyInfo.cs). The `AssemblyVersion` attribute supports wildcards (`*`) for automatic build and revision numbers.

````csharp
// flutterArbEditor/AssemblyInfo.cs
[assembly: AssemblyVersion("1.0.*")]