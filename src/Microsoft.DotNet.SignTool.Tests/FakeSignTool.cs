// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Diagnostics;
using System.IO;
using System.IO.Packaging;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using System.Collections.Generic;

namespace Microsoft.DotNet.SignTool
{
    internal sealed class FakeSignTool : SignTool
    {
        private static readonly byte[] _stamp = Guid.NewGuid().ToByteArray();

        internal FakeSignTool(SignToolArgs args, TaskLoggingHelper log)
            : base(args, log)
        {
        }

        public override void RemoveStrongNameSign(string assemblyPath)
        {
        }

        public override bool LocalStrongNameSign(IBuildEngine buildEngine, int round, IEnumerable<FileSignInfo> files) => true;

        public override bool VerifySignedPEFile(Stream stream)
        {
            var buffer = new byte[_stamp.Length];
            return stream.TryReadAll(buffer, 0, buffer.Length) == buffer.Length && 
                   ByteSequenceComparer.Equals(buffer, _stamp);
        }

        public override bool VerifyStrongNameSign(string fileFullPath)
        {
            return true;
        }

        public override bool RunMSBuild(IBuildEngine buildEngine, string projectFilePath, string binLogPath)
            => buildEngine.BuildProjectFile(projectFilePath, null, null, null);

        internal static void SignFile(string path)
        {
            // Cannot fake sign the engine as that invalidates the exe and makes it non-executable
            // which will cause insignia to be unable to reattach it to the installer exe.
            if (FileSignInfo.IsPEFile(path) &&
                !path.EndsWith(SignToolConstants.MsiEngineExtension))
            {
                SignPEFile(path);
            }
        }

        private static void SignPEFile(string path)
        {
            using (var stream = File.OpenWrite(Uri.UnescapeDataString(path)))
            {
                stream.Write(_stamp, 0, _stamp.Length);
            }
        }

        public override bool VerifySignedDeb(TaskLoggingHelper log, string filePath)
        {
            return true;
        }

        public override bool VerifySignedPowerShellFile(string filePath)
        {
            return true;
        }

        public override bool VerifySignedNugetFileMarker(string filePath)
        {
            return true;
        }

        public override bool VerifySignedVSIXFileMarker(string filePath)
        {
            return true;
        }

        public override bool VerifySignedPkgOrAppBundle(string filePath, string pkgToolPath)
        {
            return true;
        }

        protected override string GetZipFilePath(string zipFileDir, string fileName)
        {
            // The original implementation of this method creates a temporary directory
            // This is non-deterministic when testing, so we use a known path instead
            string tempDir = Path.Combine(zipFileDir, "temp");
            Directory.CreateDirectory(tempDir);
            return Path.Combine(tempDir, Path.GetFileNameWithoutExtension(fileName) + ".zip");
        }
    }
}
