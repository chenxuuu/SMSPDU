using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SMSPDU
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }


        static string MakePdu(string textsms, string telnumber)
        {
            string result = "";

            telnumber = telnumber.Replace("-", "").Replace(" ", "").Replace("+", "");

            // 01 это PDU Type или иногда называется SMS-SUBMIT. 01 означает, что сообщение передаваемое, а не получаемое 
            // цифры 00 это TP-Message-Reference означают, что телефон/модем может установить количество успешных сообщений автоматически
            // telnumber.Length.ToString("X2") выдаст нам длинну номера в 16-ричном формате
            // 91 означает, что используется международный формат номера телефона
            telnumber = "01" + "00" + telnumber.Length.ToString("X2") + "91" + EncodePhoneNumber(telnumber);

            textsms = StringToUCS2(textsms);
            // 00 означает, что формат сообщения неявный. Это идентификатор протокола. Другие варианты телекс, телефакс, голосовое сообщение и т.п.
            // 08 означает формат UCS2 - 2 байта на символ. Он проще, так что рассмотрим его.
            // если вместо 08 указать 18, то сообщение не будет сохранено на телефоне. Получится flash сообщение
            string leninByte = (textsms.Length / 2).ToString("X2");
            textsms = telnumber + "00" + "08" + leninByte + textsms;

            // посылаем команду с длинной сообщения - количество октет в десятичной системе. то есть делим на два количество символов в сообщении
            // если октет неполный, то получится в результате дробное число. это дробное число округляем до большего
            double lenMes = textsms.Length / 2;
            result += "AT+CMGS=" + (Math.Ceiling(lenMes)).ToString() + "\r\n";

            // номер sms-центра мы не указываем, считая, что практически во всех SIM картах он уже прописан
            // для того, чтобы было понятно, что этот номер мы не указали добавляем к нашему сообщению в начало 2 нуля
            // добавляем именно ПОСЛЕ того, как подсчитали длинну сообщения
            textsms = "00" + textsms;

            return result + textsms + "<CTRL + Z>(0x1A)";
        }



        // перекодирование номера телефона для формата PDU
        public static string EncodePhoneNumber(string PhoneNumber)
        {
            string result = "";
            if ((PhoneNumber.Length % 2) > 0) PhoneNumber += "F";

            int i = 0;
            while (i < PhoneNumber.Length)
            {
                result += PhoneNumber[i + 1].ToString() + PhoneNumber[i].ToString();
                i += 2;
            }
            return result.Trim();
        }


        // перекодирование текста смс в UCS2 
        public static string StringToUCS2(string str)
        {
            UnicodeEncoding ue = new UnicodeEncoding();
            byte[] ucs2 = ue.GetBytes(str);

            int i = 0;
            while (i < ucs2.Length)
            {
                byte b = ucs2[i + 1];
                ucs2[i + 1] = ucs2[i];
                ucs2[i] = b;
                i += 2;
            }
            return BitConverter.ToString(ucs2).Replace("-", "");
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            at.Text = MakePdu(sms.Text, tel.Text);
        }
    }
}
