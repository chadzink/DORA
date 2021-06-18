using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace DORA.Navigation.Context.Entities
{
    [Table("navigation_item")]
    public class NavItem
    {
        [Key]
        [Column("id")]
        [JsonProperty("id")]
        public Guid? Id { get; set; }

        // Grouping
        [Column("nav_group_id")]
        [JsonProperty("navGroupId")]
        [ForeignKey(nameof(NavGroup))]
        public Guid NavGroupId { get; set; }

        [JsonProperty("navGroup")]
        public NavGroup NavGroup { get; set; }

        // Tree structure
        [Column("parent_nav_item_id")]
        [JsonProperty("parentNavItemId")]
        [ForeignKey(nameof(ParentNavItem))]
        public Guid? ParentNavItemId { get; set; }

        [JsonProperty("parentNavItem")]
        public NavItem ParentNavItem { get; set; }

        [NotMapped]
        [JsonProperty("childNavItems")]
        public ICollection<NavItem> ChildNavItems { get; set; }

        // Display/Client
        [Column("label")]
        [JsonProperty("label")]
        public string Label { get; set; }

        [Column("key")]
        [JsonProperty("key")]
        public string Key { get; set; }

        [Column("url")]
        [JsonProperty("url")]
        public string Url { get; set; }

        [Column("target")]
        [JsonProperty("target")]
        public string UrlTarget { get; set; }

        [Column("on_click")]
        [JsonProperty("onClick")]
        public string onClickJsHandler{ get; set; }
    }
}
