using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Text.RegularExpressions;
using NetFwTypeLib;
using System.Net.NetworkInformation;
using System.Net;

namespace IKI_RAN_Test_Task
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        INetFwRule firewallRule = null;

        public MainWindow()
        {
            InitializeComponent();
        }

        // Ввод только чисел
        private new void PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            int port;
            int.TryParse(PortTextBox.Text, out port);
            if(port != 0)
            {
                // Проверка порта на доступность
                if(CheckPort(port))
                {
                    try
                    {
                        firewallRule = (INetFwRule)Activator.CreateInstance(
                        Type.GetTypeFromProgID("HNetCfg.FWRule"));

                        firewallRule.Action = NET_FW_ACTION_.NET_FW_ACTION_ALLOW;
                        firewallRule.Name = "IKI RAN Test task rule";
                        firewallRule.Description = "IKI RAN Test task rule";
                        firewallRule.Direction = NET_FW_RULE_DIRECTION_.NET_FW_RULE_DIR_IN;
                        firewallRule.Enabled = true;
                        firewallRule.InterfaceTypes = "All";
                        firewallRule.Protocol = 6; // TCP = 6
                        firewallRule.LocalPorts = port.ToString();

                        INetFwPolicy2 firewallPolicy = (INetFwPolicy2)Activator.CreateInstance(
                            Type.GetTypeFromProgID("HNetCfg.FwPolicy2"));
                        firewallPolicy.Rules.Add(firewallRule);
                        MessageBox.Show("Правило успешно создано");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"При создании правила произошла ошибка: {ex.Message}",
                            "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                else
                {
                    MessageBox.Show($"Порт {port} недоступен", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }   
            }
            else
            {
                MessageBox.Show("Неверно указан порт", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Проверка порта на доступность
        private static bool CheckPort(int port)
        {
            bool portFree = true;

            IPEndPoint[] ipEndPoints = (IPGlobalProperties.GetIPGlobalProperties()).GetActiveTcpListeners();

            foreach (IPEndPoint endPoint in ipEndPoints)
            {
                Console.WriteLine(endPoint.Port);
                if (endPoint.Port == port)
                {
                    portFree = false;
                    break;
                }
            }
            return portFree;
        }

        // Удаление правила при закрытии программы
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if(firewallRule != null)
            {
                INetFwPolicy2 firewallPolicy = (INetFwPolicy2)Activator.CreateInstance(
                    Type.GetTypeFromProgID("HNetCfg.FwPolicy2"));
                foreach (INetFwRule rule in firewallPolicy.Rules)
                {
                    if (rule.Name == "IKI RAN Test task rule")
                    {
                        firewallPolicy.Rules.Remove("IKI RAN Test task rule");
                    }
                }
            }
        }
    }
}
