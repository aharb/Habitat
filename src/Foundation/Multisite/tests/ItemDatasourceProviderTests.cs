﻿using System;
using System.Collections.Generic;
namespace Sitecore.Foundation.Multisite.Tests
{
  using System.Linq;
  using System.Text;
  using System.Threading.Tasks;
  using FluentAssertions;
  using NSubstitute;
  using Ploeh.AutoFixture.AutoNSubstitute;
  using Ploeh.AutoFixture.Xunit2;
  using Sitecore.Data;
  using Sitecore.Data.Items;
  using Sitecore.FakeDb;
  using Sitecore.Foundation.Multisite.Providers;
  using Sitecore.Foundation.Multisite.Tests.Extensions;
  using Xunit;

  public class ItemDatasourceProviderTests
  {
    [Theory]
    [AutoDbData]
    public void GetDatasources_ShouldReturnSourcesFromSettingItem([Frozen]ISettingsProvider settingsProvider, [Greedy]ItemDatasourceProvider provider, string name, Item contextItem, Db db, string settingItemName, Item item, DbItem sourceRoot)
    {
      provider.Database = db.Database;
      var settingId = ID.NewID;
      db.Add(new DbItem(settingItemName, settingId, Templates.DatasourceConfiguration.ID) {new DbField(Templates.DatasourceConfiguration.Fields.DatasourceLocation) { {"en", sourceRoot.ID.ToString()} } });
      db.Add(sourceRoot);
      var sourceRootItem = db.GetItem(sourceRoot.ID);
      var settingItem = db.GetItem(settingId);
      settingsProvider.GetSettingItem(Arg.Any<string>(), Arg.Any<Item>()).Returns(settingItem);
      var sources = provider.GetDatasources(name, item);
      sources.Should().NotBeNull();
      sources.Should().Contain(sourceRootItem);
    }

    [Theory]
    [AutoDbData]
    public void GetDatasources_LocationSetByQuery_ShouldReturnSourcesFromSettingItem([Frozen]ISettingsProvider settingsProvider, [Greedy]ItemDatasourceProvider provider, string name, Item contextItem, Db db, string settingItemName, Item item, string sourceRootName)
    {
      var sourceRoot = new DbItem(sourceRootName.Replace("-", string.Empty));
      provider.Database = db.Database;
      var settingId = ID.NewID;
      db.Add(sourceRoot);
      var sourceRootItem = db.GetItem(sourceRoot.ID);
      db.Add(new DbItem(settingItemName, settingId, Templates.DatasourceConfiguration.ID) { new DbField(Templates.DatasourceConfiguration.Fields.DatasourceLocation) { { "en", $"query:{sourceRootItem.Paths.Path}" } } });
      var settingItem = db.GetItem(settingId);
      settingsProvider.GetSettingItem(Arg.Any<string>(), Arg.Any<Item>()).Returns(settingItem);
      var sources = provider.GetDatasources(name, item);
      sources.Should().NotBeNull();
      sources.Should().Contain(sourceRootItem);
    }

    [Theory]
    [AutoDbData]
    public void GetDatasources_LocationSetByRelativePath_ShouldReturnSourcesFromSettingItem([Frozen]ISettingsProvider settingsProvider, [Greedy]ItemDatasourceProvider provider, string name, string contextItemName, Db db, string settingItemName, Item item, string sourceRootName)
    {
      var contextItemId = ID.NewID;
      var contextDbItem = new DbItem(contextItemName.Replace("-", String.Empty), contextItemId);
      
      var rootName = sourceRootName.Replace("-", string.Empty);
      var sourceRoot = new DbItem(rootName);
      contextDbItem.Add(sourceRoot);
      db.Add(contextDbItem);
      provider.Database = db.Database;
      var settingId = ID.NewID;
      var settingDbItem = new DbItem(settingItemName.Replace("-", String.Empty), settingId, Templates.DatasourceConfiguration.ID)
      {
        new DbField(Templates.DatasourceConfiguration.Fields.DatasourceLocation)
        {
          {
            "en", $"./{rootName}"
          }
        }
      };
      var contextItem = db.GetItem(contextItemId);
      db.Add(settingDbItem);
      var sourceRootItem = db.GetItem(sourceRoot.ID);
      var settingItem = db.GetItem(settingId);
      settingsProvider.GetSettingItem(Arg.Any<string>(), Arg.Any<Item>()).Returns(settingItem);
      var sources = provider.GetDatasources(name, contextItem);
      sources.Should().NotBeNull();
      sources.Should().Contain(sourceRootItem);
    }

    [Theory]
    [AutoDbData]
    public void GetDatasources_LocationSetByRelativeQuery_ShouldReturnSourcesFromSettingItem([Frozen]ISettingsProvider settingsProvider, [Greedy]ItemDatasourceProvider provider, string name, string contextItemName, Db db, string settingItemName, Item item, string sourceRootName)
    {
      var contextItemId = ID.NewID;
      var contextDbItem = new DbItem(contextItemName.Replace("-", String.Empty), contextItemId);

      var rootName = sourceRootName.Replace("-", string.Empty);
      var sourceRoot = new DbItem(rootName);
      contextDbItem.Add(sourceRoot);
      db.Add(contextDbItem);
      provider.Database = db.Database;
      var settingId = ID.NewID;
      var settingDbItem = new DbItem(settingItemName.Replace("-", String.Empty), settingId, Templates.DatasourceConfiguration.ID)
      {
        new DbField(Templates.DatasourceConfiguration.Fields.DatasourceLocation)
        {
          {
            "en", $"query:./{rootName}"
          }
        }
      };
      var contextItem = db.GetItem(contextItemId);
      db.Add(settingDbItem);
      var sourceRootItem = db.GetItem(sourceRoot.ID);
      var settingItem = db.GetItem(settingId);
      settingsProvider.GetSettingItem(Arg.Any<string>(), Arg.Any<Item>()).Returns(settingItem);
      var sources = provider.GetDatasources(name, contextItem);
      sources.Should().NotBeNull();
      sources.Should().Contain(sourceRootItem);
    }

    [Theory]
    [AutoDbData]
    public void GetDatasourceTemplate_ShouldReturnTemplateFromSettingItem([Frozen]ISettingsProvider settingsProvider, [Greedy]ItemDatasourceProvider provider, string name, Item contextItem, Db db, string settingItemName, Item item, DbItem sourceTemplate)
    {
      provider.Database = db.Database;
      var settingId = ID.NewID;
      db.Add(new DbItem(settingItemName, settingId) { new DbField(Templates.DatasourceConfiguration.Fields.DatasourceTemplate) { { "en", sourceTemplate.ID.ToString() } } });
      db.Add(sourceTemplate);
      var sourceRootItem = db.GetItem(sourceTemplate.ID);
      var settingItem = db.GetItem(settingId);
      settingsProvider.GetSettingItem(Arg.Any<string>(), Arg.Any<Item>()).Returns(settingItem);
      var sources = provider.GetDatasourceTemplate(name, item);
      sources.Should().NotBeNull();
      sources.ID.ShouldBeEquivalentTo(sourceRootItem.ID);
    }

    [Theory]
    [AutoDbData]
    public void GetDatasourceTemplate_ShouldReturnNull(ISettingsProvider settingsProvider, ItemDatasourceProvider itemDsProvider, string settingName, Item contextItem)
    {
      var provider = new ItemDatasourceProvider(settingsProvider);
      var template = provider.GetDatasourceTemplate(settingName, contextItem);
    }
  }
}