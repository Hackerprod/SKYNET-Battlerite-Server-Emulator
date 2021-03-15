using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Bson.Serialization.Options;
using MongoDB.Bson.Serialization.Serializers;
using System;
using System.Collections.Generic;

namespace SKYNET.Db
{
	public class UnsignedConventions : IConvention, IMemberMapConvention
	{
		public class UnsignedInternal : StructSerializerBase<ulong>, IRepresentationConfigurable<UnsignedInternal>, IRepresentationConverterConfigurable<UnsignedInternal>, IRepresentationConfigurable, IRepresentationConverterConfigurable
		{
			private readonly BsonType bsonType_0;

			private readonly RepresentationConverter representationConverter_0;

			public RepresentationConverter Converter => representationConverter_0;

			public BsonType Representation => bsonType_0;

			public UnsignedInternal()
			{
			}

			public UnsignedInternal(BsonType representation)
			{
			}

			public UnsignedInternal(BsonType representation, RepresentationConverter converter)
			{
				
				
				if ((uint)(representation - 1) > 1u && representation != BsonType.Int32 && (uint)(representation - 18) > 1u)
				{
					throw new ArgumentException($"{representation} is not a valid representation for a MyUInt64Serializer.");
				}
				bsonType_0 = representation;
				representationConverter_0 = converter;
			}

			public override ulong Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
			{
				if (context.Reader.CurrentBsonType != BsonType.Null)
				{
					IBsonReader reader = context.Reader;
					BsonType currentBsonType = reader.GetCurrentBsonType();
					switch (currentBsonType)
					{
					case BsonType.String:
						return JsonConvert.ToUInt64(reader.ReadString() ?? "0");
					case BsonType.Double:
						return representationConverter_0.ToUInt64(reader.ReadDouble());
					case BsonType.Int32:
						return representationConverter_0.ToUInt64(reader.ReadInt32());
					case BsonType.Int64:
						return representationConverter_0.ToUInt64(reader.ReadInt64());
					case BsonType.Decimal128:
						return representationConverter_0.ToUInt64(reader.ReadDecimal128());
					case BsonType.Undefined:
					case BsonType.Null:
						return 0uL;
					}
				}
				context.Reader.ReadNull();
				return 0uL;
			}

			public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, ulong value)
			{
				IBsonWriter writer = context.Writer;
				switch (bsonType_0)
				{
				case BsonType.Int32:
					writer.WriteInt32(representationConverter_0.ToInt32(value));
					break;
				default:
					throw new BsonSerializationException($"'{bsonType_0}' is not a valid UInt64 representation.");
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

			public UnsignedInternal WithConverter(RepresentationConverter converter)
			{
				if (converter == representationConverter_0)
				{
					return this;
				}
				return new UnsignedInternal(bsonType_0, converter);
			}

			public UnsignedInternal WithRepresentation(BsonType representation)
			{
				if (representation == bsonType_0)
				{
					return this;
				}
				return new UnsignedInternal(representation, representationConverter_0);
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

		public string Name => "AlwaysConvertUlongToStringConvention";

		public void Apply(BsonMemberMap memberMap)
		{
			if (memberMap.MemberType == typeof(ulong))
			{
				memberMap.SetSerializer(new UnsignedInternal(BsonType.String, new RepresentationConverter(allowOverflow: true, allowTruncation: true)));
			}
			else if (memberMap.MemberType == typeof(uint))
			{
				memberMap.SetSerializer(new MongoConverters(BsonType.Int64, new RepresentationConverter(allowOverflow: true, allowTruncation: true)));
			}
			else if (memberMap.MemberType == typeof(List<ulong>))
			{
				memberMap.SetSerializer(new EnumerableInterfaceImplementerSerializer<List<ulong>>(new UInt64Serializer(BsonType.String, new RepresentationConverter(allowOverflow: true, allowTruncation: true))));
			}
			else if (memberMap.MemberType == typeof(List<uint>))
			{
				memberMap.SetSerializer(new EnumerableInterfaceImplementerSerializer<List<uint>>(new UInt32Serializer(BsonType.Int64, new RepresentationConverter(allowOverflow: true, allowTruncation: true))));
			}
		}


		public UnsignedConventions()
		{
			
			
		}
	}
}
