(**
F# Data Toolbox
===================

F# Data Toolbox is a library for various data access APIs based on FSharp.Data. It
will contain individual packages for each data source.

The first package for Twitter access is available through NuGet.

<div class="row">
  <div class="span1"></div>
  <div class="span6">
    <div class="well well-small" id="nuget">
      The F# Data Toolbox library can be <a href="https://nuget.org/packages/FSharp.Data.Toolbox.Twitter">installed from NuGet</a>:
      <pre>PM> Install-Package FSharp.Data.Toolbox.Twitter</pre>
    </div>
  </div>
  <div class="span1"></div>
</div>


Samples & documentation
-----------------------

The library currently includes the Twitter type provider for access to Twitter users
and feeds, and SAS type provider to read SAS dataset files. 

 * [Twitter type provider](TwitterProvider.html) contains examples on how to use this toolbox to access Twitter.
 * [SAS type provider](SasProvider.html) contains examples on how to use this toolbox to access SAS datasets.
 * [BIS type provider](BisProvider.html) contains examples on how to use this toolbox to access Bank for International Settlements datasets.

 
Contributing and copyright
--------------------------

The project is hosted on [GitHub][gh] where you can [report issues][issues], fork 
the project and submit pull requests. If you're adding new public API, please also 
consider adding [samples][content] that can be turned into a documentation. You might
also want to read [library design notes][readme] to understand how it works.

The library is available under Public Domain license, which allows modification and 
redistribution for both commercial and non-commercial purposes. For more information see the 
[License file][license] in the GitHub repository. 

  [content]: https://github.com/fsprojects/FSharp.Data.Toolbox/tree/master/docs 
  [gh]: https://github.com/fsprojects/FSharp.Data.Toolbox
  [issues]: https://github.com/fsprojects/FSharp.Data.Toolbox/issues
  [readme]: https://github.com/fsprojects/FSharp.Data.Toolbox/blob/master/README.md
  [license]: https://github.com/fsprojects/FSharp.Data.Toolbox/blob/master/LICENSE.txt
*)

