namespace HIES.FO
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private async void Application_Startup(object sender, StartupEventArgs e)
        {
            var SplashScreen = new Screens.Shared.SplashScreen();
            this.MainWindow = SplashScreen;
            SplashScreen.Show();
            
            //Check initilisation of tables needed for first time
            try
            {
                //First Check Database Connection
                var xWindow = new Screens.Shared.ConnectToDatabase();
                xWindow.ShowDialog();
                if (!xWindow.IsDatabaseConnectionSucessful)
                {
					Current?.Shutdown();
                }

                await Task.Factory.StartNew(() =>
                {
                    using var InitialiseDB = new API.FO.Shared.InitialCustomisation();
                    InitialiseDB.InitialiseLocalApplicationDatabase();
                });

                bool ForceDeveloperLogin = false;
                if (!API.FO.Developers.Client.IsClientDetailsExist())
                {
                    ForceDeveloperLogin = true;
                }
                if (!API.FO.Developers.Properties.IsPropertiesDetailsExist())
                {
                    ForceDeveloperLogin = true;
                }

                if (ForceDeveloperLogin)
                {
                    MessageBox.Show("Database has not been initialised, kindly ask software vendor to initialiase the same!");
                }

                var LoginWindow = new Screens.Authorisation.Login();
                this.MainWindow = LoginWindow;
                LoginWindow.Closing += LoginWindow_Closing;
                LoginWindow.Show();

                SplashScreen.Close();

                //Register syncfusion licence
                Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("MjU5NDQwMUAzMjMyMmUzMDJlMzBkSGZscWY5dGpTSlBBMEFQOUlmcjhmYldQakZ6eVVPejRVRlhiS0Q2UytFPQ==;NRAiBiAaIQQuGjN/V0R+XU9HclRDX3xKf0x/TGpQb19xflBPallYVBYiSV9jS31TcUdhWXxaeHFUT2hZUA==;MjU5NDQwM0AzMjMyMmUzMDJlMzBhM1dGQVZjMjVqQW9mMk1nbk12L09JeWxFdDhSUCt1Y0RKSzFvOTNuMmMwPQ==;MjU5NDQwNEAzMjMyMmUzMDJlMzBFNDlWTW5FVlRvVnRyazV1S2pPUGh4YkFaVVFnb2pBaFVKUGp3OXR6REMwPQ==;MjU5NDQwNUAzMjMyMmUzMDJlMzBIOFh5cnRneEU5TlYvZVB5bU1WZkJsZ0tEamNTN05zOUFFVzdCbU1BUlhnPQ==");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error:-\n" +
                                ex.Message + "\n" +
                                ex.InnerException, "Error in initialising database", MessageBoxButton.OK,
                        MessageBoxImage.Error);

                if (Application.Current != null) { Application.Current.Shutdown(); }
                return;
            }
        }

        private void LoginWindow_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                if (sender != null)
                {
                    var LoginWindow = (Screens.Authorisation.Login)sender;

                    if (LoginWindow.MoveToDeveloperLoginWindow)
                    {
                        var LoginWindow1 = new Screens.Developers.Login();
                        LoginWindow1.ShowDialog();

                        if (LoginWindow1.IsLoginSuccessful)
                        {
                            var MainWindow1 = new Screens.Developers.DeveloperPanel(LoginWindow1.LoggedInUserID);
                            this.MainWindow = MainWindow1;
                            MainWindow1.Show();
                        }
                    }
                    else
                    {
                        if (LoginWindow.IsLoginSuccessful)
                        {
                            string PropertyID = LoginWindow.AuthorisedPropertyID;

                            if (PropertyID == "")
                            {
                                throw new Exception("Application has not been customised propertly, kindly contact vendor!");
                            }
                            var MainWindow = new Screens.Shared.MainWindow(LoginWindow.LoggedInUserID, PropertyID);
                            this.MainWindow = MainWindow;
                            MainWindow.Show();
                        }
                        else
                        {
                            //Application.Current.Shutdown();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error-" + ex.Message + "\n Inner Exception- " + ex.InnerException, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Application.Current.Shutdown();
            }
        }

    }
}
