// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using NUnit.Framework;

namespace Rdmp.Core.Tests.DataLoad.Engine.Integration;

[Category("Unit")]
public class WebFileDownloaderTests
{
    [Test, Ignore("Let's not put usernames and password in here eh.")]
    public void ProxyTest()
    {
        var url = "http://www.bbc.co.uk/news";

        Stream response;
        try
        {
            response = CreateRequest(url);
        }
        catch (Exception e)
        {
            // First retry with proxy credentials
            Console.WriteLine($"First HTTP request failed with '{e.Message}', retrying with credentials");
            var credentials = new NetworkCredential("proxyUsername", "proxyPassword");
            response = CreateRequest(url, credentials);
        }

        var bytesRead=0;
        while (response.ReadByte() != -1)
            bytesRead++;

        response.Close();
        Assert.Greater(bytesRead, 0);
    }
    

    private static readonly HttpClientHandler HttpClientHandler = new ();
    private static readonly HttpClient HttpClient=new(HttpClientHandler,true);

    private static Stream CreateRequest(string url, ICredentials credentials=null)
    {
        lock (HttpClient)
        {
            if (credentials is not null)
            {
                HttpClientHandler.Credentials = credentials;
                HttpClientHandler.PreAuthenticate = true;
            }
            using var cts=new CancellationTokenSource(5000);
            return HttpClient.GetStreamAsync(url,cts.Token).Result;
        }
    }
}