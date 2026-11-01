using Microsoft.VisualStudio.Shell;
using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace CodeLinkAdapterExt
{
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [Guid(PackageGuidString)]
    public sealed class CodeLinkAdapterExtPackage : AsyncPackage
    {
        public const string PackageGuidString = "23b43816-5f26-491a-82e7-0a5b15b47a18";
        protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            await OpenCodexLinkCommand.InitializeAsync(this, cancellationToken);
        }
    }
}
