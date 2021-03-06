// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using NUnit.Framework;

namespace Rdmp.Core.Tests.DataLoad.Engine.Integration
{
    [Category("Unit")]
    public class WebFileDownloaderTests
    {
        [Test, Ignore("Let's not put usernames and password in here eh.")]
        public void ProxyTest()
        {
            string url = "http://www.bbc.co.uk/news";

            HttpWebRequest request = CreateRequest(url);
                
            string tempFileName = Path.GetTempFileName();
            try
            {
                using (var writer = new StreamWriter(tempFileName))
                {
                    WebResponse response;

                    try
                    {
                        response = request.GetResponse();
                        Console.WriteLine("Did not receive a websense challenge :-(");
                    }
                    catch (WebException e)
                    {
                        if (
                            //if there is a WWW-Authenticate header && the contents of that header are websense related
                            e.Response.Headers.AllKeys.Contains("WWW-Authenticate") && e.Response.Headers["WWW-Authenticate"].Equals("Basic realm=\"Websense\""))
                            Console.WriteLine("received " + e.Status +" and a header containing WWW-Authenticate instructions to go to URL " + e.Response.ResponseUri.AbsoluteUri);
                        else
                            throw;//something else went wrong

                        //make a request to the websense proxy
                        request = CreateRequest(e.Response.ResponseUri.AbsoluteUri);
                        //todo put your friend and mine Mr Kernthaler in here:
                        request.Credentials = new NetworkCredential("proxyUsername", "proxyPassword");
                        response = request.GetResponse();
                        
                        Console.WriteLine("Websense authentication worked, preparing to re-send original request");
                        
                        //now make another request for the original page
                        request = CreateRequest(url);
                        response = request.GetResponse();
                    }

                    using (var responseStream = response.GetResponseStream())
                    {
                        // Process the stream
                        byte[] buf = new byte[16384];

                        int countReadSoFar = 0;
                        int count = 0;
                        do
                        {
                            count = responseStream.Read(buf, 0, buf.Length);

                            if (count != 0)
                            {
                                writer.Write(Encoding.ASCII.GetString(buf, 0, count));
                                writer.Flush();
                                countReadSoFar += count;

                                Console.WriteLine("Wrote " + count + " bytes");

                            }
                        } while (count > 0);

                        responseStream.Close();
                        response.Close();
                        writer.Close();
                    }
                    
                }

                Assert.Greater((new FileInfo(tempFileName)).Length, 0);
            }
            finally
            {
                File.Delete(tempFileName);
            }
        }

        private HttpWebRequest CreateRequest(string url)
        {
            var request =(HttpWebRequest)WebRequest.Create(url);
            request.Timeout = 5000; // milliseconds, adjust as needed
            request.UserAgent =
                "Nobody";
            request.ReadWriteTimeout = 10000; // milliseconds, adjust as needed


            request.Credentials = new NetworkCredential();
            request.PreAuthenticate = true;
            return request;
        }
    }
}
