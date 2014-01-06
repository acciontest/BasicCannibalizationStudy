using System;
using System.Collections.Generic;
using AlteryxGalleryAPIWrapper;
using HtmlAgilityPack;
using Newtonsoft.Json;
using NUnit.Framework;
using TechTalk.SpecFlow;

namespace BasicCannibalizationStudy
{
    [Binding]
    public class BasicCannibalizationStudySteps
    {
        private string alteryxurl;
        private string _sessionid;
        private string _appid;
        private string _userid;
        private string _appName;
        private string jobid;
        private string outputid;
        private string validationId;
        private string _appActualName;
        private dynamic statusresp;
    


        //   private Client Obj = new Client("https://devgallery.alteryx.com/api/");
        private Client Obj = new Client("https://gallery.alteryx.com/api/");

        private RootObject jsString = new RootObject();

        [Given(@"alteryx running at""(.*)""")]
        public void GivenAlteryxRunningAt(string SUT_url)
        {
            alteryxurl = Environment.GetEnvironmentVariable(SUT_url);
        }

        [Given(@"I am logged in using ""(.*)"" and ""(.*)""")]
        public void GivenIAmLoggedInUsingAnd(string user, string password)
        {
            _sessionid = Obj.Authenticate(user, password).sessionId;
        }
        
        [When(@"I run the App ""(.*)"" with (.*) and predefined Existing Store Drivetime")]
        public void WhenIRunTheAppWithAndPredefinedExistingStoreDrivetime(string app, int p1)
        {
            //url + "/apps/gallery/?search=" + appName + "&limit=20&offset=0"
            //Search for App & Get AppId & userId 
            string response = Obj.SearchAppsGallery(app);
            var appresponse =
                new System.Web.Script.Serialization.JavaScriptSerializer().Deserialize<Dictionary<string, dynamic>>(
                    response);
            int count = appresponse["recordCount"];
            if (count == 1)
            {
                _appid = appresponse["records"][0]["id"];
                _userid = appresponse["records"][0]["owner"]["id"];
                _appName = appresponse["records"][0]["primaryApplication"]["fileName"];
            }
            else
            {
                for (int i = 0; i <= count - 1; i++)
                {

                    _appActualName = appresponse["records"][i]["primaryApplication"]["metaInfo"]["name"];
                    if (_appActualName == app)
                    {
                        _appid = appresponse["records"][i]["id"];
                        _userid = appresponse["records"][i]["owner"]["id"];
                        _appName = appresponse["records"][i]["primaryApplication"]["fileName"];
                        break;
                    }
                }

            }
            jsString.appPackage.id = _appid;
            jsString.userId = _userid;
            jsString.appName = _appName;
            //url +"/apps/" + appPackageId + "/interface/
            //Get the app interface - not required
            string appinterface = Obj.GetAppInterface(_appid);
            dynamic interfaceresp = JsonConvert.DeserializeObject(appinterface);

            //Construct the payload to be posted.
            List<JsonPayload.Question> questionAnsls = new List<JsonPayload.Question>();
            questionAnsls.Add(new JsonPayload.Question("potentialStoreDrivetime", p1.ToString()));
           
            jsString.questions.AddRange(questionAnsls);
            jsString.jobName = "Job Name";

            // Make Call to run app

            var postData = new System.Web.Script.Serialization.JavaScriptSerializer().Serialize(jsString);
            string postdata = postData.ToString();
            string resjobqueue = Obj.QueueJob(postdata);

            var jobqueue =
                new System.Web.Script.Serialization.JavaScriptSerializer().Deserialize<Dictionary<string, dynamic>>(
                    resjobqueue);
            jobid = jobqueue["id"];

            //Get the job status

            int count1 = 0;
            string status = "";
            while (status != "Completed")
            {
                System.Threading.Thread.Sleep(500);
                string jobstatusresp = Obj.GetJobStatus(jobid);
                var statusResponse =
                    new System.Web.Script.Serialization.JavaScriptSerializer().Deserialize<Dictionary<string, dynamic>>(
                        jobstatusresp);
                status = statusResponse["status"];
            }
        }
        
        [Then(@"I see the  amount of cannibalized sales in the output ""(.*)""")]
        public void ThenISeeTheAmountOfCannibalizedSalesInTheOutput(string amount)
        {
            //url + "/apps/jobs/" + jobId + "/output/"
            string getmetadata = Obj.GetOutputMetadata(jobid);
            dynamic metadataresp = JsonConvert.DeserializeObject(getmetadata);

            // outputid = metadataresp[0]["id"];
            int count = metadataresp.Count;
            for (int j = 0; j <= count - 1; j++)
            {
                outputid = metadataresp[j]["id"];
            }

            string getjoboutput = Obj.GetJobOutput(jobid, outputid, "html");
            string htmlresponse = getjoboutput;
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(htmlresponse);
            string output = doc.DocumentNode.SelectSingleNode("//span[@class='DefaultNumericText']").InnerHtml;
            string output1 = Math.Ceiling(Convert.ToDecimal(output)).ToString();
          //  decimal finaloutput = Math.Truncate(output1);
            StringAssert.Contains(amount, output1);
        }
    }
}
