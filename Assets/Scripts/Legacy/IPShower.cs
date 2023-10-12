using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Net;
using System.Net.Sockets;
using System.Net.NetworkInformation;

public class IPShower : MonoBehaviour
{
    public TMP_Text m_TextComponent;
    // Start is called before the first frame update
    void Update()
    {
        var host = Dns.GetHostEntry(Dns.GetHostName());
        foreach (var ip in host.AddressList)
        {
            if (ip.AddressFamily == AddressFamily.InterNetwork)
            {
                m_TextComponent.text =  $"IP: {ip.ToString()}";
                return;
            }
        }
        throw new System.Exception("No network adapters with an IPv4 address in the system!");
    }
}
