﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using OelianderUI.Views;
using Newtonsoft.Json;
using System.Net.Http;

namespace OelianderUI.Helpers;

public class ShodanScanner
{
    public static ShodanResponse sr = null;
    public static bool _continue = false;
    public static void GetShodanSearch()
    {
        var version = ScanHelper._osVersion;
        using var wc = new HttpClient();
        var json = wc.GetStringAsync($"https://api.shodan.io/shodan/host/search?key={MainPage.settings.Shodan_API_Key}&query=\"{MainPage.settings.Shodan_Pattern}\"+\"{version}\"").Result;
        //string json = wc.DownloadString($"https://api.shodan.io/shodan/host/search?key={MainPage.settings.Shodan_API_Key}&query=\"{MainPage.settings.Shodan_Pattern}\"+\"{version}\"");
        sr = JsonConvert.DeserializeObject<ShodanResponse>(json);
        _continue = true;
    }
}


public class Location
{
    public string city { get; set; }
    public string region_code { get; set; }
    public object area_code { get; set; }
    public double longitude { get; set; }
    public string country_name { get; set; }
    public string country_code { get; set; }
    public double latitude { get; set; }
}

public class Match
{
    public Shodan _shodan { get; set; }
    public int hash { get; set; }
    public string os { get; set; }
    public DateTime timestamp { get; set; }
    public string isp { get; set; }
    public string asn { get; set; }
    public List<string> hostnames { get; set; }
    public Location location { get; set; }
    public object ip { get; set; }
    public List<string> domains { get; set; }
    public string org { get; set; }
    public string data { get; set; }
    public int port { get; set; }
    public string transport { get; set; }
    public string ip_str { get; set; }
}

public class Options
{
    public string scan { get; set; }
}

public class ShodanResponse
{
    public List<Match> matches { get; set; }
    public int total { get; set; }
}

public class Shodan
{
    public string id { get; set; }
    public bool ptr { get; set; }
    public Options options { get; set; }
    public string module { get; set; }
    public string crawler { get; set; }
}
