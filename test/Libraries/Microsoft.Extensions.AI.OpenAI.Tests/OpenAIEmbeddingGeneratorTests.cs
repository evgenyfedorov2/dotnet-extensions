﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.ClientModel;
using System.ClientModel.Primitives;
using System.Net.Http;
using System.Threading.Tasks;
using Azure.AI.OpenAI;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using OpenAI;
using OpenAI.Embeddings;
using Xunit;

#pragma warning disable S103 // Lines should not be too long

namespace Microsoft.Extensions.AI;

public class OpenAIEmbeddingGeneratorTests
{
    [Fact]
    public void Ctor_InvalidArgs_Throws()
    {
        Assert.Throws<ArgumentNullException>("openAIClient", () => new OpenAIEmbeddingGenerator(null!, "model"));
        Assert.Throws<ArgumentNullException>("embeddingClient", () => new OpenAIEmbeddingGenerator(null!));

        OpenAIClient openAIClient = new("key");
        Assert.Throws<ArgumentNullException>("modelId", () => new OpenAIEmbeddingGenerator(openAIClient, null!));
        Assert.Throws<ArgumentException>("modelId", () => new OpenAIEmbeddingGenerator(openAIClient, ""));
        Assert.Throws<ArgumentException>("modelId", () => new OpenAIEmbeddingGenerator(openAIClient, "   "));
    }

    [Fact]
    public void AsEmbeddingGenerator_InvalidArgs_Throws()
    {
        Assert.Throws<ArgumentNullException>("openAIClient", () => ((OpenAIClient)null!).AsEmbeddingGenerator("model"));
        Assert.Throws<ArgumentNullException>("embeddingClient", () => ((EmbeddingClient)null!).AsEmbeddingGenerator());

        OpenAIClient client = new("key");
        Assert.Throws<ArgumentNullException>("modelId", () => client.AsEmbeddingGenerator(null!));
        Assert.Throws<ArgumentException>("modelId", () => client.AsEmbeddingGenerator("   "));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void AsEmbeddingGenerator_OpenAIClient_ProducesExpectedMetadata(bool useAzureOpenAI)
    {
        Uri endpoint = new("http://localhost/some/endpoint");
        string model = "amazingModel";

        var client = useAzureOpenAI ?
            new AzureOpenAIClient(endpoint, new ApiKeyCredential("key")) :
            new OpenAIClient(new ApiKeyCredential("key"), new OpenAIClientOptions { Endpoint = endpoint });

        IEmbeddingGenerator<string, Embedding<float>> embeddingGenerator = client.AsEmbeddingGenerator(model);
        Assert.Equal("openai", embeddingGenerator.Metadata.ProviderName);
        Assert.Equal(endpoint, embeddingGenerator.Metadata.ProviderUri);
        Assert.Equal(model, embeddingGenerator.Metadata.ModelId);

        embeddingGenerator = client.GetEmbeddingClient(model).AsEmbeddingGenerator();
        Assert.Equal("openai", embeddingGenerator.Metadata.ProviderName);
        Assert.Equal(endpoint, embeddingGenerator.Metadata.ProviderUri);
        Assert.Equal(model, embeddingGenerator.Metadata.ModelId);
    }

    [Fact]
    public void GetService_OpenAIClient_SuccessfullyReturnsUnderlyingClient()
    {
        OpenAIClient openAIClient = new(new ApiKeyCredential("key"));
        IEmbeddingGenerator<string, Embedding<float>> embeddingGenerator = openAIClient.AsEmbeddingGenerator("model");

        Assert.Same(embeddingGenerator, embeddingGenerator.GetService<IEmbeddingGenerator<string, Embedding<float>>>());
        Assert.Same(embeddingGenerator, embeddingGenerator.GetService<OpenAIEmbeddingGenerator>());

        Assert.Same(openAIClient, embeddingGenerator.GetService<OpenAIClient>());

        Assert.NotNull(embeddingGenerator.GetService<EmbeddingClient>());

        using IEmbeddingGenerator<string, Embedding<float>> pipeline = new EmbeddingGeneratorBuilder<string, Embedding<float>>()
            .UseOpenTelemetry()
            .UseDistributedCache(new MemoryDistributedCache(Options.Options.Create(new MemoryDistributedCacheOptions())))
            .Use(embeddingGenerator);

        Assert.NotNull(pipeline.GetService<DistributedCachingEmbeddingGenerator<string, Embedding<float>>>());
        Assert.NotNull(pipeline.GetService<CachingEmbeddingGenerator<string, Embedding<float>>>());
        Assert.NotNull(pipeline.GetService<OpenTelemetryEmbeddingGenerator<string, Embedding<float>>>());

        Assert.Same(openAIClient, pipeline.GetService<OpenAIClient>());
        Assert.IsType<OpenTelemetryEmbeddingGenerator<string, Embedding<float>>>(pipeline.GetService<IEmbeddingGenerator<string, Embedding<float>>>());
    }

    [Fact]
    public void GetService_EmbeddingClient_SuccessfullyReturnsUnderlyingClient()
    {
        EmbeddingClient openAIClient = new OpenAIClient(new ApiKeyCredential("key")).GetEmbeddingClient("model");
        IEmbeddingGenerator<string, Embedding<float>> embeddingGenerator = openAIClient.AsEmbeddingGenerator();

        Assert.Same(embeddingGenerator, embeddingGenerator.GetService<IEmbeddingGenerator<string, Embedding<float>>>());
        Assert.Same(openAIClient, embeddingGenerator.GetService<EmbeddingClient>());

        using IEmbeddingGenerator<string, Embedding<float>> pipeline = new EmbeddingGeneratorBuilder<string, Embedding<float>>()
            .UseOpenTelemetry()
            .UseDistributedCache(new MemoryDistributedCache(Options.Options.Create(new MemoryDistributedCacheOptions())))
            .Use(embeddingGenerator);

        Assert.NotNull(pipeline.GetService<DistributedCachingEmbeddingGenerator<string, Embedding<float>>>());
        Assert.NotNull(pipeline.GetService<CachingEmbeddingGenerator<string, Embedding<float>>>());
        Assert.NotNull(pipeline.GetService<OpenTelemetryEmbeddingGenerator<string, Embedding<float>>>());

        Assert.Same(openAIClient, pipeline.GetService<EmbeddingClient>());
        Assert.IsType<OpenTelemetryEmbeddingGenerator<string, Embedding<float>>>(pipeline.GetService<IEmbeddingGenerator<string, Embedding<float>>>());
    }

    [Fact]
    public async Task GetEmbeddingsAsync_ExpectedRequestResponse()
    {
        const string Input = """
            {"input":["hello, world!","red, white, blue"],"model":"text-embedding-3-small","encoding_format":"base64"}
            """;

        const string Output = """
            {
              "object": "list",
              "data": [
                {
                  "object": "embedding",
                  "index": 0,
                  "embedding": "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"
                },
                {
                  "object": "embedding",
                  "index": 1,
                  "embedding": "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"
                }
              ],
              "model": "text-embedding-3-small",
              "usage": {
                "prompt_tokens": 9,
                "total_tokens": 9
              }
            }
            """;

        using VerbatimHttpHandler handler = new(Input, Output);
        using HttpClient httpClient = new(handler);
        using IEmbeddingGenerator<string, Embedding<float>> generator = new OpenAIClient(new ApiKeyCredential("apikey"), new OpenAIClientOptions
        {
            Transport = new HttpClientPipelineTransport(httpClient),
        }).AsEmbeddingGenerator("text-embedding-3-small");

        var response = await generator.GenerateAsync([
            "hello, world!",
            "red, white, blue",
        ]);
        Assert.NotNull(response);
        Assert.Equal(2, response.Count);

        Assert.NotNull(response.Usage);
        Assert.Equal(9, response.Usage.InputTokenCount);
        Assert.Equal(9, response.Usage.TotalTokenCount);

        foreach (Embedding<float> e in response)
        {
            Assert.Equal("text-embedding-3-small", e.ModelId);
            Assert.NotNull(e.CreatedAt);
            Assert.Equal(1536, e.Vector.Length);
            Assert.Contains(e.Vector.ToArray(), f => !f.Equals(0));
        }
    }
}