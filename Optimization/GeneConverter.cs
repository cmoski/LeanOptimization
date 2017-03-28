﻿using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Globalization;

namespace Optimization
{

    public class GeneConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(GeneConfiguration);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
            {
                return null;
            }

            var json = JObject.Load(reader);

            var precision = json["precision"]?.Value<int?>();

            GeneConfiguration gene = new GeneConfiguration
            {
                Key = json["key"].Value<string>(),
                MinDecimal = precision > 0 ? json["min"].Value<decimal?>() : null,
                MaxDecimal = precision > 0 ? json["max"].Value<decimal?>() : null,
                MinInt = precision > 0 ? null : json["min"].Value<int?>(),
                MaxInt = precision > 0 ? null: json["max"].Value<int?>(),
                Precision = precision
            };
            if (json["actual"] != null)
            {

                int parsed;
                string raw = json["actual"].Value<string>();
                if (int.TryParse(raw, out parsed))
                {
                    gene.ActualInt = parsed;
                }

                if (!gene.ActualInt.HasValue)
                {
                    decimal decimalParsed;
                    if (decimal.TryParse(raw, out decimalParsed))
                    {
                        gene.ActualDecimal = decimalParsed;
                    }
                    if (decimal.TryParse(raw, NumberStyles.Float, CultureInfo.InvariantCulture, out decimalParsed))
                    {
                        gene.ActualDecimal = decimalParsed;
                    }
                }
            }

            return gene;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var gene = (GeneConfiguration)value;

            serializer.Serialize(writer, gene.Key);
            serializer.Serialize(writer, gene.Precision);
            if (gene.ActualDecimal.HasValue)
            {
                writer.WritePropertyName("min");
                writer.WriteValue(gene.MinDecimal);
                writer.WritePropertyName("max");
                writer.WriteValue(gene.MaxDecimal);
                writer.WritePropertyName("actual");
                writer.WriteValue(gene.ActualDecimal);
            }
            if (gene.ActualInt.HasValue)
            {
                writer.WritePropertyName("min");
                writer.WriteValue(gene.MinInt);
                writer.WritePropertyName("max");
                writer.WriteValue(gene.MaxInt);
                writer.WritePropertyName("actual");
                writer.WriteValue(gene.ActualInt);
            }

        }

    }


}