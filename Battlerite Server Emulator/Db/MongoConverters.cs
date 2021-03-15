using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Options;
using MongoDB.Bson.Serialization.Serializers;
using System;

namespace SKYNET.Db
{
	public class MongoConverters : StructSerializerBase<uint>, IRepresentationConfigurable<MongoConverters>, IRepresentationConverterConfigurable<MongoConverters>, IRepresentationConfigurable, IRepresentationConverterConfigurable
	{
		private readonly BsonType bsonType_0;

		private readonly RepresentationConverter representationConverter_0;

		public RepresentationConverter Converter => representationConverter_0;

		public BsonType Representation => bsonType_0;

		public MongoConverters()
		{
		}

		public MongoConverters(BsonType representation)
		{
		}

		public MongoConverters(BsonType representation, RepresentationConverter converter)
		{
			if ((uint)(representation - 1) > 1u && representation != BsonType.Int32 && (uint)(representation - 18) > 1u)
			{
				throw new ArgumentException($"{representation} is not a valid representation for a MyUInt32Serializer.");
			}
			bsonType_0 = representation;
			representationConverter_0 = converter;
		}

		public override uint Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
		{
			if (context.Reader.CurrentBsonType != BsonType.Null)
			{
				IBsonReader reader = context.Reader;
				BsonType currentBsonType = reader.GetCurrentBsonType();
				switch (currentBsonType)
				{
				case BsonType.String:
					return JsonConvert.ToUInt32(reader.ReadString() ?? "0");
				case BsonType.Double:
					return representationConverter_0.ToUInt32(reader.ReadDouble());
				case BsonType.Int32:
					return representationConverter_0.ToUInt32(reader.ReadInt32());
				case BsonType.Int64:
					return representationConverter_0.ToUInt32(reader.ReadInt64());
				case BsonType.Decimal128:
					return representationConverter_0.ToUInt32(reader.ReadDecimal128());
				case BsonType.Undefined:
				case BsonType.Null:
					return 0u;
				}
			}
			context.Reader.ReadNull();
			return 0u;
		}

		public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, uint value)
		{
			IBsonWriter writer = context.Writer;
			switch (bsonType_0)
			{
			case BsonType.Int32:
				writer.WriteInt32(representationConverter_0.ToInt32(value));
				break;
			default:
				throw new BsonSerializationException($"'{bsonType_0}' is not a valid UInt32 representation.");
			case BsonType.Int64:
				writer.WriteInt64(representationConverter_0.ToInt64(value));
				break;
			case BsonType.Decimal128:
				writer.WriteDecimal128(representationConverter_0.ToDecimal128(value));
				break;
			case BsonType.String:
				writer.WriteString(JsonConvert.ToString(value));
				break;
			case BsonType.Double:
				writer.WriteDouble(representationConverter_0.ToDouble(value));
				break;
			}
		}

		public MongoConverters WithConverter(RepresentationConverter converter)
		{
			if (converter == representationConverter_0)
			{
				return this;
			}
			return new MongoConverters(bsonType_0, converter);
		}

		public MongoConverters WithRepresentation(BsonType representation)
		{
			if (representation == bsonType_0)
			{
				return this;
			}
			return new MongoConverters(representation, representationConverter_0);
		}

		IBsonSerializer IRepresentationConverterConfigurable.WithConverter(RepresentationConverter converter)
		{
			return WithConverter(converter);
		}

		IBsonSerializer IRepresentationConfigurable.WithRepresentation(BsonType representation)
		{
			return WithRepresentation(representation);
		}

	}
}
