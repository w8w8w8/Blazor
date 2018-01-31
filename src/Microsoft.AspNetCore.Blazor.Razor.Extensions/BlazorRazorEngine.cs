﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using Microsoft.AspNetCore.Razor.Language;
using Microsoft.AspNetCore.Razor.Language.Extensions;
using Microsoft.AspNetCore.Blazor.Components;
using System.Linq;
using System.Collections.Generic;

namespace Microsoft.AspNetCore.Blazor.Razor
{
    /// <summary>
    /// Wraps <see cref="RazorEngine"/>, configuring it to compile Blazor components.
    /// </summary>
    public class BlazorRazorEngine
    {
        private readonly RazorEngine _engine;
        private readonly RazorCodeGenerationOptions _codegenOptions;

        public RazorEngine Engine => _engine;

        public BlazorRazorEngine(IEnumerable<BlazorComponentDescriptor> componentDescriptors)
        {
            _codegenOptions = RazorCodeGenerationOptions.CreateDefault();

            _engine = RazorEngine.Create(configure =>
            {
                FunctionsDirective.Register(configure);
                InheritsDirective.Register(configure);
                InjectDirective.Register(configure);
                TemporaryLayoutPass.Register(configure);
                TemporaryImplementsPass.Register(configure);
                configure.Features.Add(new BlazorComponentsTagHelperFeature(componentDescriptors));
                configure.Features.Remove(
                    configure.Features.Single(f => f.GetType().Name == "PreallocatedTagHelperAttributeOptimizationPass"));

                configure.SetBaseType(BlazorComponent.FullTypeName);

                configure.Phases.Remove(
                    configure.Phases.OfType<IRazorCSharpLoweringPhase>().Single());
                configure.Phases.Add(new BlazorLoweringPhase(_codegenOptions));

                configure.ConfigureClass((codeDoc, classNode) =>
                {
                    configure.SetNamespace((string)codeDoc.Items[BlazorCodeDocItems.Namespace]);
                    classNode.ClassName = (string)codeDoc.Items[BlazorCodeDocItems.ClassName];
                });
            });
        }
    }
}
