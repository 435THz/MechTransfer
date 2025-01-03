﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.UI;
using Terraria.GameContent.UI.Elements;
using Terraria.Localization;
using Terraria.Map;
using Terraria.UI;

namespace MechTransfer.UI
{
    public class AssemblerHoverUI : UIState
    {
        public bool visible = false;

        private UIPanel panel;
        private UIText itemText;
        private UIText titleText;
        private UIText stationText;
        private UIText atText;
        private Item[] fakeInv = new Item[11];
        private Item[] ingrRow = [];
        private float lowestSlot;

        private const string NotSetTextKey = "Mods.MechTransfer.UI.Hover.NotSet";

        public override void OnInitialize()
        {
            fakeInv[10] = new Item();

            titleText = new UIText(Language.GetTextValue(NotSetTextKey));
            titleText.Left.Set(5, 0);
            titleText.Top.Set(5, 0);
            Append(titleText);

            panel = new UIPanel();
            panel.Left.Set(0, 0);
            panel.Top.Set(25, 0);
            panel.Width.Set(200, 0);
            panel.Height.Set(TextureAssets.InventoryBack.Value.Height * 0.5f * Main.UIScale + 10, 0);

            itemText = new UIText(Language.GetTextValue(NotSetTextKey));
            itemText.Left.Set(TextureAssets.InventoryBack.Value.Width * 0.5f * Main.UIScale, 0);
            itemText.Top.Set(0, 0);
            panel.Append(itemText);

            atText = new UIText("@ ");
            atText.Left.Set(10, 0);
            atText.TextOriginX = 0f;
            panel.Append(atText);

            stationText = new UIText("");
            stationText.Left.Set(10 + atText.MinWidth.Pixels, 0);
            stationText.TextOriginX = 0f;
            panel.Append(stationText);

            Append(panel);
        }

        public void Display(Item type, List<Item> ingredients, List<int> stations, string text, Color textColor)
        {
            visible = true;

            titleText.SetText(text);
            titleText.TextColor = textColor;

            fakeInv[10] = type;
            ingredients ??= [];
            ingrRow = new Item[10 + ingredients.Count];
            for (int i = 0; i < ingredients.Count; i++) ingrRow[i + 10] = ingredients[i];

            itemText.SetText(type.IsAir ? Language.GetTextValue(NotSetTextKey) : fakeInv[10].Name);
            itemText.TextColor = type.IsAir ? Color.Red : ItemRarity.GetColor(fakeInv[10].rare);
            panel.Width.Pixels = itemText.MinWidth.Pixels + TextureAssets.InventoryBack.Value.Width * 0.5f + 25;
            int rows = (ingrRow.Length - 1) / 10;
            panel.Width.Pixels = Math.Max(panel.Width.Pixels, (int)Math.Ceiling((ingrRow.Length - 10.0) / (rows==0?1:rows)) * (TextureAssets.InventoryBack.Value.Width + 4) * 0.5f + 20);
            stationText.Width.Set(panel.GetInnerDimensions().Width-5, 0f);

            string statnText = "";
            string atTextString = "";
            float statnHeight = 0;
            int statnLines = 0;
            if (stations.Count > 0)
            {
                UIText stationTextBox = new UIText(",");
                float semicolonWidth = stationTextBox.MinWidth.Pixels;
                atTextString = "@ ";
                DynamicSpriteFont font = FontAssets.MouseText.Value;
                float spacing = font.LineSpacing;
                statnText = "";
                for (int i = 0; i < stations.Count; i++)
                {
                    int id = stations[i];
                    if (i != 0) statnText += ",\n";
                    string stationName = Lang.GetMapObjectName(MapHelper.TileToLookup(id, Recipe.GetRequiredTileStyle(id)));
                    statnText += stationName;

                    stationTextBox.SetText(stationName);
                    panel.Width.Pixels = Math.Max(panel.Width.Pixels, stationTextBox.MinWidth.Pixels + stationText.Left.Pixels + 20 + (i < stations.Count - 1 ? semicolonWidth : 0));
                }

                statnLines = statnText.Split('\n').Length;
                statnHeight = statnLines * spacing;
            }
            atText.SetText(atTextString);
            stationText.SetText(statnText);
            panel.Height.Pixels = statnHeight + (TextureAssets.InventoryBack.Value.Width + ((rows == 0 ? 0 : (ingrRow.Length - 10) % rows + 1) * (TextureAssets.InventoryBack.Value.Height + 4))) * 0.5f + 10;
            float top = lowestSlot;
            atText.Top.Set(top, 0f);
            stationText.Top.Set(top, 0f);
            
            Vector2 pos = Vector2.Transform(Main.MouseScreen, Main.GameViewMatrix.TransformationMatrix);

            Left.Pixels = (pos.X + 10) / Main.UIScale;
            Top.Pixels = (pos.Y + 10) / Main.UIScale;

            Recalculate();
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (!visible)
                return;

            base.Draw(spriteBatch);

            lowestSlot = TextureAssets.InventoryBack.Value.Height * 0.5f * Main.UIScale + 10;
            float oldScale = Main.inventoryScale;
            Main.inventoryScale = 0.5f;
            ItemSlot.Draw(spriteBatch, fakeInv, ItemSlot.Context.InventoryItem, 10, new Vector2(Left.Pixels + 5, Top.Pixels + 30));
            for (int i=10; i<ingrRow.Length; i++)
            {
                int j = i - 10;
                int rows = ((ingrRow.Length - 10) / 10) +1;
                int cols = (int)Math.Ceiling((ingrRow.Length - 10.0) / rows);
                int x = j % cols;
                int y = j / cols;
                ItemSlot.Draw(spriteBatch, ingrRow, ItemSlot.Context.InventoryItem, i, new Vector2(Left.Pixels + 5 + (x * 28)+5, Top.Pixels + 56 + (y * 28)+2));
                lowestSlot = ((y+1) * 28) + TextureAssets.InventoryBack.Value.Height * 0.5f;
            }
            Main.inventoryScale = oldScale;

            visible = false;
        }
    }
}