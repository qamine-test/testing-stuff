# SonarC#

|Product|Quality Gate|Coverage|
|:--:|:--:|:--:|
|Analyzer|[![Quality Gate](https://next.sonarqube.com/sonarqube/api/badges/gate?key=sonaranalyzer-csharp-vbnet)](https://next.sonarqube.com/sonarqube/dashboard?id=sonaranalyzer-csharp-vbnet)|[![Coverage](https://next.sonarqube.com/sonarqube/api/badges/measure?key=sonaranalyzer-csharp-vbnet&metric=coverage)](https://next.sonarqube.com/sonarqube/component_measures/domain/Coverage?id=sonaranalyzer-csharp-vbnet)|
|Plugin|[![Quality Gate](https://next.sonarqube.com/sonarqube/api/badges/gate?key=org.sonarsource.dotnet%3Asonar-csharp)](https://next.sonarqube.com/sonarqube/dashboard?id=org.sonarsource.dotnet%3Asonar-csharp)|[![Coverage](https://next.sonarqube.com/sonarqube/api/badges/measure?key=org.sonarsource.dotnet%3Asonar-csharp&metric=coverage)](https://next.sonarqube.com/sonarqube/component_measures/domain/Coverage?id=org.sonarsource.dotnet%3Asonar-csharp)|

SonarC# is a [static code analyser](https://en.wikipedia.org/wiki/Static_program_analysis) for C# language used as an extension for the [SonarQube](http://www.sonarqube.org/) platform. It will allow you to produce stable and easily supported code by helping you to find and to correct bugs, vulnerabilities and smells in your code.

## Features

* 240+ rules (including 80+ bug detection)
* Metrics (complexity, number of lines etc.)
* Import of [test coverage reports](https://docs.sonarqube.org/x/CoBh) from Visual Studio Code Coverage, dotCover, OpenCover and NCover 3.
* Support for [custom rules](https://github.com/SonarSource-VisualStudio/sonarqube-roslyn-sdk)

## Useful links

* [Project homepage](https://redirect.sonarsource.com/plugins/csharp.html)
* [Issue tracking](https://github.com/SonarSource/sonar-csharp/issues)

## Have question or feedback?

To provide feedback (request a feature, report a bug etc.), simply [create a GitHub Issue](https://github.com/SonarSource/sonar-csharp/issues).

## How to contribute

Check out the [contributing](CONTRIBUTING.md) page to see the best places to log issues and start discussions.

## How to build

To build the plugin embedding a C# analyzer that was built locally:

    mvn clean install -P local-analyzer -D analyzer.configuration=Debug

This option is useful for developers in Windows,
who build the C# analyzer themselves.

---

To build the plugin embedding an already built C# analyzer:

    mvn clean install -P download-analyzer -D analyzer.version=6.1.0.2070

This option is useful for developers in Unix,
who don't build the C# analyzer themselves,
because they work on other parts of the plugin.

## License

Copyright 2014-2017 SonarSource.


Licensed under the [GNU Lesser General Public License, Version 3.0](http://www.gnu.org/licenses/lgpl.txt)
