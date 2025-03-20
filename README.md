# Oeliander Vulnerability Analysis Framework

*NOTE* - Oeliander may stay as a dedicated MikroTik ExpFramework and the new work-in-progress may be moved to a new repo

```
A vulnearbility analysis framework with the ability to use shodan to gather targets.
```

# Vulnerability Scanner Integrations
*Ported to C#*

- MikroTik WinBox Auth Bypass Credential Disclosure [CVE-2018-14847]
- F5 Big-IP Remote Code Execution Vulnerability [CVE-2023-46747]
- Juniper SRX Firewall Vulnerability [CVE-2023-36845]
- SpringCore Remote Code Execution Vulnearbility [CVE-2020-5405]
- OpenFire Console Authentication Bypass Vulnerability [CVE-2023-3215]
- Remote Unauthenticated Code Execution Vulnerability in OpenSSH server [CVE-2024-6387]
- Kibana RCE [CVE-2019-7609]

## TODO
- VMWare Aria Operations for Networks (vRealize Network Insight) unauthenticated RCE [CVE-2023-20887]
- VMWare ESXi RCE Exploit [CVE2021-21974]
- Atlassian Bitbucket Data Center Deserialization Vulnerability [CVE-2022-26133]
- ConnectWise ScreenConnect-AuthBypass-RCE [CVE-2024-1708][CVE-2024-1709]
- Zimbra Remote Code Execution Vulnerability [CVE-2022-27925]
- Apache OFBiz Remote Code Execution Vulnerability [CVE-2024-38856]
- Log4J [CVE-2021-45046]
- Apache Commons Text RCE [CVE-2022-42889]
- Webmin Unauthorized Remote Code Execution Vulnerability (<=1.920) [CVE-2019-15107]
- integrate shodan searches for each vulnerability

Expanding from MikroTik RouterOS Vulnerability Analysis framework to encompass a wider array of vulnearbilities to be able to detect

<p align="center">
	<img align="center" src="https://raw.githubusercontent.com/whiterabb17/Oeliander/master/Screenshot.png">
</p>

<a href="https://github.com/whiterabb17/MkCheck">MkCheck</a> given an easy to use UI
