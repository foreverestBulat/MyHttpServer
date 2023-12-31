﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyHttpServer
{
    public class AppSettingsConfig
    {
        public int Port { get; set; }
        public string Address { get; set; }
        public string StaticPathFiles { get; set; }
        public string EmailFrom { get; set; }
        public string EmailTo { get; set; }
        public string EmailPassword { get; set; }
        public string SmtpHost { get; set; }
        public int SmtpPort { get; set; }
    }
}
