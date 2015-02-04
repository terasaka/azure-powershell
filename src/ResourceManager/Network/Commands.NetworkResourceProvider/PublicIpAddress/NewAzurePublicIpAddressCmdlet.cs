﻿

// ----------------------------------------------------------------------------------
//
// Copyright Microsoft Corporation
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// ----------------------------------------------------------------------------------

using System;
using System.Collections;
using System.Management.Automation;
using AutoMapper;
using Microsoft.Azure.Management.Network;
using Microsoft.Azure.Commands.NetworkResourceProvider.Models;
using Microsoft.Azure.Commands.NetworkResourceProvider.Properties;
using Microsoft.WindowsAzure.Commands.Utilities.Common;
using MNM = Microsoft.Azure.Management.Network.Models;

namespace Microsoft.Azure.Commands.NetworkResourceProvider
{
    [Cmdlet(VerbsCommon.New, "AzurePublicIpAddress"), OutputType(typeof(PSPublicIpAddress))]
    public class NewAzurePublicIpAddressCmdlet : PublicIpAddressBaseClient
    {
        [Alias("ResourceName")]
        [Parameter(
            Mandatory = false,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "The resource name.")]
        [ValidateNotNullOrEmpty]
        public virtual string Name { get; set; }

        [Parameter(
            Mandatory = true,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "The resource group name.")]
        [ValidateNotNullOrEmpty]
        public virtual string ResourceGroupName { get; set; }

        [Parameter(
            Mandatory = false,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "The public IP address location.")]
        [ValidateNotNullOrEmpty]
        public string Location { get; set; }

        [Parameter(
            Mandatory = true,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "The public IP address allocation method.")]
        [ValidateNotNullOrEmpty]
        [ValidateSet(MNM.IpAllocationMethod.Dynamic, IgnoreCase = true)]
        public string AllocationMethod { get; set; }

        [Parameter(
            Mandatory = false,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "The Domain Name label.")]
        public string DomainNameLabel { get; set; }

        [Parameter(
            Mandatory = false,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "IdleTimeoutInMinutes")]
        public int IdleTimeoutInMinutes { get; set; }

        [Alias("Tags")]
        [Parameter(
            Mandatory = false,
            ValueFromPipelineByPropertyName = true,
            HelpMessage = "An array of hashtables which represents resource tags.")]
        public Hashtable[] Tag { get; set; }

        [Parameter(
            Mandatory = false,
            HelpMessage = "Do not ask for confirmation if you want to overrite a resource")]
        public SwitchParameter Force { get; set; }

        public override void ExecuteCmdlet()
        {
            base.ExecuteCmdlet();

            if (this.IsPublicIpAddressPresent(this.ResourceGroupName, this.Name))
            {
                ConfirmAction(
                    Force.IsPresent,
                    string.Format(Resources.OverwritingResource, Name),
                    Resources.OverwritingResourceMessage,
                    Name,
                    () => CreatePublicIpAddress());
            }

            var publicIp = CreatePublicIpAddress();

            WriteObject(publicIp);
        }

        private PSPublicIpAddress CreatePublicIpAddress()
        {
            var publicIp = new PSPublicIpAddress();
            publicIp.Name = this.Name;
            publicIp.Location = this.Location;
            publicIp.Properties = new PSPublicIpAddressProperties();
            publicIp.Properties.PublicIpAllocationMethod = this.AllocationMethod;

            if (this.IdleTimeoutInMinutes > 0)
            {
                publicIp.Properties.IdleTimeoutInMinutes = this.IdleTimeoutInMinutes;
            }

            if (!string.IsNullOrEmpty(this.DomainNameLabel))
            {
                publicIp.Properties.DnsSettings = new PSPublicIpAddressDnsSettings();
                publicIp.Properties.DnsSettings.DomainNameLabel = this.DomainNameLabel;
            }

            var publicIpModel = Mapper.Map<MNM.PublicIpAddressCreateOrUpdateParameters>(publicIp);

            publicIpModel.Tags = TagsConversionHelper.CreateTagDictionary(this.Tag, validate: true);

            this.PublicIpAddressClient.CreateOrUpdate(this.ResourceGroupName, this.Name, publicIpModel);

            var getPublicIp = this.GetPublicIpAddress(this.ResourceGroupName, this.Name);

            return getPublicIp;
        }
    }
}

 