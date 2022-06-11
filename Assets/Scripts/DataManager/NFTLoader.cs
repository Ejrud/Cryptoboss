using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class NFTLoader : MonoBehaviour
{
    public Item items;

    public class Creator
    {
        public string account { get; set; }
        public int value { get; set; }
    }

    public class Attribute
    {
        public string key { get; set; }
        public string value { get; set; }
    }

    public class Content
    {
        public string @type { get; set; }
        public string url { get; set; }
        public string representation { get; set; }
        public string mimeType { get; set; }
    }

    public class Meta
    {
        public string name { get; set; }
        public string description { get; set; }
        public IList<Attribute> attributes { get; set; }
        public IList<Content> content { get; set; }
        public IList<object> restrictions { get; set; }
    }

    public class Item
    {
        public string id { get; set; }
        public string blockchain { get; set; }
        public string collection { get; set; }
        public string contract { get; set; }
        public string tokenId { get; set; }
        public IList<Creator> creators { get; set; }
        public IList<object> owners { get; set; }
        public IList<object> royalties { get; set; }
        public string lazySupply { get; set; }
        public IList<object> pending { get; set; }
        public DateTime mintedAt { get; set; }
        public DateTime lastUpdatedAt { get; set; }
        public string supply { get; set; }
        public Meta meta { get; set; }
        public bool deleted { get; set; }
        public IList<object> auctions { get; set; }
        public string totalStock { get; set; }
        public int sellers { get; set; }
    }

    public class Data
    {
        public int total { get; set; }
        public IList<Item> items { get; set; }
    }

    public class Nfts
    {
        public Data data { get; set; }
    }
}
