using Cyberliberty.Guzzle;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GuzzleUnitTests
{
    [TestClass]
    public class UnitTest1
    {

        /// <summary>
        /// This should always be 200 OK (unless the server is down.)
        /// </summary>
        [TestMethod]
        public void SendSync200OkGetRequest()
        {
            /* init instance */
            Guzzle guzzle = new Guzzle( "http://api.safe-harbor.me/" );

            /* send request */
            var response = guzzle.Get();

            /* check results */
            Assert.IsTrue((response.Status == 200), "200 OK.");
        }

        /// <summary>
        /// This should always be 404 Not Found (unless the server is down.)
        /// </summary>
        [TestMethod]
        public void SendSync404NotFoundGetRequest()
        {
            /* init instance */
            Guzzle guzzle = new Guzzle("http://api.safe-harbor.me/this-route-not-found");

            /* send request */
            var response = guzzle.Get();

            /* check results */
            Assert.IsTrue((response.Status == 404), "404 Not Found.");
        }

        /// <summary>
        /// This should always PASS unless some idiot bought this ridiculous domain, 
        /// which would not be my fault.
        /// </summary>
        [TestMethod]
        public void SendSyncGetRequestToUnreachableDomain()
        {
            /* init instance */
            Guzzle guzzle = new Guzzle("http://duh.domain-unreachable.ever/");

            /* send request */
            var response = guzzle.Get();

            /* check results */
            Assert.IsNotInstanceOfType(response, typeof(Response), "Not A Response Object.");
        }

        /// <summary>
        /// This should always give an error, due to the expired link.
        /// </summary>
        [TestMethod]
        public void Download1MbFileWithExpiredLink()
        {
            /* init instance */
            Guzzle guzzle = new Guzzle("http://cdownload.me/d/0a3f29a7e8f51489a1847ec54129ba22/100?e=1636116757");

            /* add session header */
            guzzle.AddHeader("Cookie", "PHPSESSID=hltcc3st4dphupkkpogonha629");

            /* send request */
            var response = guzzle.Head();

            /* check results */
            Assert.AreNotEqual(1048576, response.Content.Length);
        }
    }
}
