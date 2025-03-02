﻿# Azure DevOps AI Evaluation Report

This extension is designed to work with the reports created by the [`Microsoft.Extensions.AI.Evaluation.Reporting`](https://www.nuget.org/packages/Microsoft.Extensions.AI.Evaluation.Reporting) library. This extension contains a task for publishing the data along with an extension that will display the report in a tab in your build pipeline results.

# Publishing the data

The evaluation data must be in the JSON format generated by the AI Evaluation libraries. You can use either the `Microsoft.Extensions.AI.Evaluation.Reporting` library directly or the `Microsoft.Extensions.AI.Evaluation.Console` tool to generate this data.

This extension publishes a `PublishAIEvaluationReport` task that will publish the data for you. Having this task in your pipeline also triggers the report tab to appear.

```yaml
  - task: PublishAIEvaluationReport@0
    displayName: 'Publish AI Evaluation Data'
    inputs:
      reportFile: '$(build-artifacts)\report.json'     
```

If your pipeline restricts uploading attachments from a task, you can disable the attachment upload feature and use a different method to upload the report data. You should still include `PublishAIEvaluationReport` task as it will trigger the display of the reporting tab. When using this method, the attachment must be uploaded to the pipeline with `type=ai-eval-report-json` and `name=ai-eval-report`.

For example,

```yaml
- task: PublishAIEvaluationReport@0
  displayName: 'Display AI Evaluation Data'
  inputs:
    reportFile: '$(build-artifacts)\report.json'
    disableAttachmentUpload: true

- script: |
    echo "##vso[task.addattachment type=ai-eval-report-json;name=ai-eval-report;]$(build-artifacts)\report.json"
```

# The Microsoft.Extensions.AI.Evaluation libraries

`Microsoft.Extensions.AI.Evaluation` is a set of .NET libraries defined in the following NuGet packages that have been designed to work together to support building processes for evaluating the quality of AI software.

* [`Microsoft.Extensions.AI.Evaluation`](https://www.nuget.org/packages/Microsoft.Extensions.AI.Evaluation) - Defines core abstractions and types for supporting evaluation.
* [`Microsoft.Extensions.AI.Evaluation.Quality`](https://www.nuget.org/packages/Microsoft.Extensions.AI.Evaluation.Quality) - Contains evaluators that can be used to evaluate the quality of AI responses in your projects including Relevance, Truth, Completeness, Fluency, Coherence, Equivalence and Groundedness.
* [`Microsoft.Extensions.AI.Evaluation.Reporting`](https://www.nuget.org/packages/Microsoft.Extensions.AI.Evaluation.Reporting) - Contains support for caching LLM responses, storing the results of evaluations and generating reports from that data.
* [`Microsoft.Extensions.AI.Evaluation.Reporting.Azure`](https://www.nuget.org/packages/Microsoft.Extensions.AI.Evaluation.Reporting.Azure) - Supports the `Microsoft.Extensions.AI.Evaluation.Reporting` library with an implementation for caching LLM responses and storing the evaluation results in an Azure Storage container.
* [`Microsoft.Extensions.AI.Evaluation.Console`](https://www.nuget.org/packages/Microsoft.Extensions.AI.Evaluation.Console) - A command line dotnet tool for generating reports and managing evaluation data.

## Install the packages

From the command-line:

```console
dotnet add package Microsoft.Extensions.AI.Evaluation
dotnet add package Microsoft.Extensions.AI.Evaluation.Quality
dotnet add package Microsoft.Extensions.AI.Evaluation.Reporting
```

Or directly in the C# project file:

```xml
<ItemGroup>
  <PackageReference Include="Microsoft.Extensions.AI.Evaluation" Version="[CURRENTVERSION]" />
  <PackageReference Include="Microsoft.Extensions.AI.Evaluation.Quality" Version="[CURRENTVERSION]" />
  <PackageReference Include="Microsoft.Extensions.AI.Evaluation.Reporting" Version="[CURRENTVERSION]" />
</ItemGroup>
```

You can optionally add the `Microsoft.Extensions.AI.Evaluation.Reporting.Azure` package in either of these places if you need Azure Storage support.

## Install the command line tool

```console
dotnet tool install Microsoft.Extensions.AI.Evaluation.Console --create-manifest-if-needed
```

## Usage Examples

For a comprehensive tour of all the functionality, concepts and APIs available in the `Microsoft.Extensions.AI.Evaluation` libraries, check out the [API Usage Examples](https://github.com/dotnet/ai-samples/blob/main/src/microsoft-extensions-ai-evaluation/api/) available in the [dotnet/ai-samples](https://github.com/dotnet/ai-samples) repo. These examples are structured as a collection of unit tests. Each unit test showcases a specific concept or API, and builds on the concepts and APIs showcased in previous unit tests.


## Feedback & Contributing

We welcome feedback and contributions in [our GitHub repo](https://github.com/dotnet/extensions).
