using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace DORA.Navigation.Context.Entities
{
    public enum NavGroupType
    {
        List,
        Bar,
        Tab,
        Button,
        Hamburger,
    }

    [Table("navigation_group")]
    public class NavGroup
    {
        [Key]
        [Column("id")]
        [JsonProperty("id")]
        public Guid? Id { get; set; }

        [Column("group_type")]
        [JsonProperty("groupType")]
        public NavGroupType GroupType { get; set; }

        [Column("label")]
        [JsonProperty("label")]
        public string Label { get; set; }

        [Column("key")]
        [JsonProperty("key")]
        public string Key { get; set; }

        [Column("default_nav_item_id")]
        [JsonProperty("defaultNavItemId")]
        [ForeignKey(nameof(DefaultNavItem))]
        public Guid? DefaultNavItemId { get; set; }

        [JsonProperty("defaultNavItem")]
        public NavItem DefaultNavItem { get; set; }

        [NotMapped]
        [JsonProperty("navItems")]
        public ICollection<NavItem> NavItems { get; set; }
    }
}
