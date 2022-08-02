using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Keys = OpenQA.Selenium.Keys;

namespace WhatsAppMsgSender
{
    public partial class Form1 : Form
    {
        static IWebDriver driver = null;
        int waittimesmall = 1000;
        public static string url = "http://wa.me/";

        public Form1()
        {
            InitializeComponent();
            worker = new BackgroundWorker();
            worker.WorkerSupportsCancellation = true;
            worker.WorkerReportsProgress = true;
            worker.DoWork += doWork;
            worker.ProgressChanged += workerProgress;
            worker.RunWorkerCompleted += Worker_RunWorkerCompleted;

            InitDriver();
        }

        private void InitDriver()
        {
            if (driver == null)
            {
                var service = ChromeDriverService.CreateDefaultService();
                service.HideCommandPromptWindow = true;

                ChromeOptions options = new ChromeOptions();
                string tfn = @"C:\Temp";

                options.AddArgument($"--user-data-dir={tfn}");
                options.AddArgument("--disable-extensions");
                options.AddArgument("--disable-gpu");
                options.AddArgument("--disable-notifications");
                options.AddArgument("--ignore-certificate-errors");
                options.AddArgument("--no-sandbox");
                options.AddArgument("--disable-dev-shm-usage");

                options.AddUserProfilePreference("credentials_enable_service", false);
                options.AddUserProfilePreference("profile.password_manager_enabled", false);

                options.AddExcludedArgument("enable-automation");
                options.AddAdditionalChromeOption("useAutomationExtension", false);

                driver = new ChromeDriver(service, options);
                driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(1);
                driver.Navigate().GoToUrl(url);
            }
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            if (btnStart.Text.Equals("start", StringComparison.OrdinalIgnoreCase))
            {
                btnStart.Text = "Stop";
                lblStatus.Text = "Runing";
                lblStatus.BackColor = Color.SkyBlue;
                lblStatus.ForeColor = Color.Black;
                timer.Start();
            }
            else if (btnStart.Text.Equals("Stop", StringComparison.OrdinalIgnoreCase))
            {
                worker.CancelAsync();
                btnStart.Text = "Start";
                lblStatus.Text = "Stoped";
                lblStatus.ForeColor = Color.White;
                lblStatus.BackColor = Color.Red;
                timer.Stop();
            }
        }

        private void doWork(object sender, DoWorkEventArgs e)
        {
            StartBrowser();
            List<Message> data = (List<Message>)e.Argument;

            if (IsConnected())
            {
                SendMessage(data, e);
            }
        }

        public void StartBrowser()
        {
            InitDriver();

            IWebElement ele = driver.FindElementSafe(By.Id("action-button"));
            if (ele != null)
            {
                ele.Click();
            }
            Task.Delay(waittimesmall).Wait();
            ele = driver.FindElementSafe(By.LinkText("use WhatsApp Web"));
            if (ele != null)
            {
                ele.Click();
            }
        }

        public bool isBrowserClosed()
        {
            bool isClosed = false;
            try
            {
                if (string.IsNullOrEmpty(driver.Title))
                    return true;
            }
            catch
            {
                isClosed = true;
            }

            return isClosed;
        }

        public void CloseBrowser()
        {
            try
            {
                if (driver != null)
                    driver.Close();
            }
            catch
            {
            }
            try
            {
                if (driver != null)
                    driver.Quit();
            }
            catch
            {
            }
            driver = null;
        }

        private bool IsConnected()
        {
            int i = 0;
            IWebElement ele = null;
            do
            {
                ele = driver.FindElementSafe(By.XPath("//header[@data-testid='chatlist-header']"));
                Task.Delay(50).Wait();
                i++;
            } while (ele == null || i > 100);
            if (ele != null)
            {
                return true;
            }
            return false;
        }

        private void SendMessage(List<Message> data, DoWorkEventArgs e)
        {
            worker.ReportProgress(-1, data.Count.ToString());
            int i = 0;
            if (IsConnected())
            {
                foreach (var item in data)
                {
                    if (worker.CancellationPending == true)
                    {
                        e.Cancel = true;
                        return;
                    }

                    worker.ReportProgress(i, "Current:" + item.Receipent);

                    if (true)//???** check if internet is connected
                    {
                        bool selected = selectPhone(item.Receipent);
                        if (selected)
                        {
                            selected = checkIfSelected(item.Receipent);
                            if (selected && item.MSGType.Equals("file"))
                            {
                                SendFile(item.Path);
                                worker.ReportProgress(1, "Processed");
                            }
                            else if (selected && item.MSGType.Equals("text"))
                            {
                                SendText(item.Text);
                                worker.ReportProgress(1, "Processed");
                            }
                        }
                        else
                        {
                            driver.Navigate().GoToUrl(url + item.Receipent);
                            IWebElement ele = driver.FindElementSafe(By.Id("action-button"));
                            ele.Click();
                            Thread.Sleep(5000);
                            IWebElement aEle = driver.FindElementSafe(By.LinkText("use WhatsApp Web"));
                            aEle.Click();
                            Thread.Sleep(10000);
                        }
                    }
                    i++;
                }
            }
        }

        private bool selectPhone(string phone)
        {
            IWebElement ele = null;
            ele = driver.FindElementSafe(By.CssSelector("span[data-testid='x-alt']"));
            if (ele != null)
                ele.Click();
            Task.Delay(waittimesmall).Wait();

            ele = driver.FindElementSafe(By.CssSelector("div[title='Search input textbox']"));

            if (ele != null)
            {
                ele.Click();
                ele.SendKeys(phone);
                Task.Delay(waittimesmall).Wait();
                ele.SendKeys(Keys.Enter);
                Task.Delay(50).Wait();

                ele = driver.FindElementSafe(By.CssSelector("div[id='pane-side']"));
                List<IWebElement> objele = ele.FindElements(By.TagName("span")).ToList();
                foreach (IWebElement objel in objele)
                {
                    try
                    {
                        if (objel.Text.ToUpper().Equals("No chats, contacts or messages found".ToUpper()))
                        {
                            return false;
                        }
                    }
                    catch (Exception)
                    {
                    }

                }
                return true;
            }
            return false;
        }

        private bool checkIfSelected(string phone)
        {
            try
            {

                IWebElement ele = driver.FindElements(By.TagName("header")).LastOrDefault();

                List<IWebElement> objIWebElement = ele.FindElements(By.TagName("span")).ToList();

                foreach (IWebElement item in objIWebElement)
                {
                    if (item.Text.ToUpper().Contains(phone.ToUpper()))
                    {
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
            }
            return false;
        }

        private string SendFile(string file)
        {
            try
            {
                if (System.IO.File.Exists(file))
                {
                    IWebElement ele = driver.FindElementSafe(By.CssSelector("div[title='Attach']"));
                    if (ele != null)
                    {
                        ele.Click();
                        Task.Delay(250).Wait();
                        Application.DoEvents();

                        ele = ele.FindElementSafe(By.XPath("//input[@accept='*']"));
                        if (ele == null)
                        {
                            Task.Delay(250).Wait();
                            ele = driver.FindElementSafe(By.CssSelector("div[title='Attach']"));
                            if (ele != null)
                            {
                                ele.Click();
                                Task.Delay(250).Wait();
                                Application.DoEvents();
                            }
                            ele = ele.FindElementSafe(By.XPath("//input[@accept='*']"));
                        }
                        if (ele != null)
                        {
                            ele.SendKeys(file);
                            Task.Delay(waittimesmall).Wait();
                            Application.DoEvents();

                            ele = driver.FindElementSafe(By.CssSelector("div[aria-label='Send']"));
                            if (ele != null)
                            {
                                ele.Click();
                                string fileName = new System.IO.FileInfo(file).Name;
                                Task.Delay(waittimesmall).Wait();
                                bool fileSentStatus = VerifyIfFileSent(fileName, "file", 30);
                                if (fileSentStatus)
                                {
                                }
                            }
                        }
                    }

                    //SendKeys.SendWait(file);
                    //SendKeys.SendWait("{Enter}");
                }
            }
            catch (Exception ex)
            {
            }
            return "not sent";
        }

        private string SendText(string text)
        {
            Task.Delay(waittimesmall).Wait();
            IWebElement ele = driver.FindElementSafe(By.XPath("//div[@title='Type a message']"));
            if (ele != null)
            {
                ele.Click();
                ele.SendKeys(text);
                ele.SendKeys(Keys.Enter);
                Task.Delay(250).Wait();
            }
            return "";
        }

        private bool VerifyIfFileSent(string textToSearch, string msgType, int retry)
        {
            IWebElement ele = driver.FindElementSafe(By.Id("main"));
            int _retry = 0;
            if (ele != null)
            {
                IWebElement tmp = null;
                do
                {
                    _retry++;
                    Task.Delay(250).Wait();
                    tmp = ele.FindElementSafe(By.XPath("//span[@data-testid='audio-cancel-noborder']"));
                    if (tmp == null)
                        tmp = ele.FindElementSafe(By.XPath("//div[@data-testid='time-left-eta']"));

                } while (tmp != null || _retry > retry);
                if (tmp == null)
                    return true;
            }
            return false;
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            if (!worker.IsBusy)
            {
                lblStatus.Text = "waiting for connection";
                lblStatus.BackColor = Color.SkyBlue;
                lblStatus.ForeColor = Color.Black;
                dataGridView.DataSource = DB.GetDate();
                worker.RunWorkerAsync(DB.GetDate());
            }
        }

        private void workerProgress(object sender, ProgressChangedEventArgs e)
        {
            // First call, the percentage is negative to signal that UserState
            // contains the number of messages we loop on....
            if (e.ProgressPercentage == -1)
            {
                progressBar.Maximum = Convert.ToInt32(e.UserState);
                lblTotal.Text = progressBar.Maximum.ToString();
                lblUnprocessed.Text = progressBar.Maximum.ToString();
                lblProcessed.Text = "0";
                lblStatus.Text = "Runing";
                lblStatus.BackColor = Color.SkyBlue;
                lblStatus.ForeColor = Color.Black;
            }
            else
            {
                if (e.UserState.ToString().StartsWith("Current:"))
                {
                    txtCurrent.Text = e.UserState.ToString().Replace("Current:", "");
                    progressBar.Value = e.ProgressPercentage;
                }
                else if (e.UserState.Equals("Processed"))
                {
                    lblProcessed.Text = (Convert.ToInt32(lblProcessed.Text) + 1).ToString();
                    lblUnprocessed.Text = (Convert.ToInt32(lblUnprocessed.Text) - 1).ToString();
                }
                //    progressBar.Value = e.ProgressPercentage;
            }
        }

        private void Worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled)
            {
                lblStatus.Text = "Cenceled";
                lblStatus.ForeColor = Color.White;
                lblStatus.BackColor = Color.Red;
                progressBar.Value = progressBar.Maximum;
                txtCurrent.Text = "";
            }
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            CloseBrowser();
        }

    }

    public static class Extension
    {
        public static IWebElement FindElementSafe(this IWebDriver driver, By by, int timeoutInSeconds = 10)
        {
            try
            {
                if (timeoutInSeconds < 10)
                    timeoutInSeconds = 10;

                return driver.FindElement(by);
            }
            catch
            {
                return null;
            }
        }

        public static IWebElement FindElementSafe(this IWebElement driver, By by, int timeoutInSeconds = 10)
        {
            try
            {
                if (timeoutInSeconds < 10)
                    timeoutInSeconds = 10;

                return driver.FindElement(by);
            }
            catch (NoSuchElementException)
            {
                return null;
            }
        }

    }
}
