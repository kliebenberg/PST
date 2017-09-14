﻿using pst.core;
using pst.encodables.messaging;
using pst.encodables.ndb;
using pst.encodables.ndb.btree;
using pst.interfaces;
using pst.interfaces.ltp;
using pst.utilities;
using System;
using System.Text;

namespace pst.impl.messaging
{
    class PropertyNameToIdMap : IPropertyNameToIdMap
    {
        private readonly IDecoder<NAMEID> nameIdDecoder;
        private readonly IPropertyContextBasedPropertyReader propertyContextBasedPropertyReader;
        private readonly IMapper<NID, Maybe<LNBTEntry>> nidToLNBTEntryMapper;

        public PropertyNameToIdMap(
            IDecoder<NAMEID> nameIdDecoder,
            IPropertyContextBasedPropertyReader propertyContextBasedPropertyReader,
            IMapper<NID, Maybe<LNBTEntry>> nidToLNBTEntryMapper)
        {
            this.nameIdDecoder = nameIdDecoder;
            this.propertyContextBasedPropertyReader = propertyContextBasedPropertyReader;
            this.nidToLNBTEntryMapper = nidToLNBTEntryMapper;
        }

        public Maybe<PropertyId> GetPropertyId(Guid propertySet, int numericalId)
        {
            var lnbtEntryForNameToIdMap =
                nidToLNBTEntryMapper.Map(Globals.NID_NAME_TO_ID_MAP);

            if (lnbtEntryForNameToIdMap.HasNoValue)
            {
                return Maybe<PropertyId>.NoValue();
            }

            var entryStream =
                propertyContextBasedPropertyReader.ReadProperty(
                    lnbtEntryForNameToIdMap.Value.DataBlockId,
                    lnbtEntryForNameToIdMap.Value.SubnodeBlockId,
                    MAPIProperties.PidTagNameidStreamEntry);

            if (entryStream.HasNoValue)
            {
                return Maybe<PropertyId>.NoValue();
            }

            var entriesCount = entryStream.Value.Value.Length / 8;

            for (var i = 0; i < entriesCount; i++)
            {
                var entry = nameIdDecoder.Decode(entryStream.Value.Value.Take(i * 8, 8));

                if (entry.Type == 0)
                {
                    if (entry.PropertyId == numericalId)
                    {
                        return Maybe<PropertyId>.OfValue(new PropertyId(entry.PropertyIndex + 0x8000));
                    }
                }
            }

            return Maybe<PropertyId>.NoValue();
        }

        public Maybe<PropertyId> GetPropertyId(Guid propertySet, string propertyName)
        {
            var lnbtEntryForNameToIdMap =
                nidToLNBTEntryMapper.Map(Globals.NID_NAME_TO_ID_MAP);

            if (lnbtEntryForNameToIdMap.HasNoValue)
            {
                return Maybe<PropertyId>.NoValue();
            }

            var entryStream =
                propertyContextBasedPropertyReader.ReadProperty(
                    lnbtEntryForNameToIdMap.Value.DataBlockId,
                    lnbtEntryForNameToIdMap.Value.SubnodeBlockId,
                    MAPIProperties.PidTagNameidStreamEntry);

            var stringStream =
                propertyContextBasedPropertyReader.ReadProperty(
                    lnbtEntryForNameToIdMap.Value.DataBlockId,
                    lnbtEntryForNameToIdMap.Value.SubnodeBlockId,
                    MAPIProperties.PidTagNameidStreamString);

            if (entryStream.HasNoValue || stringStream.HasNoValue)
            {
                return Maybe<PropertyId>.NoValue();
            }

            var entriesCount = entryStream.Value.Value.Length / 8;

            for (var i = 0; i < entriesCount; i++)
            {
                var entry = nameIdDecoder.Decode(entryStream.Value.Value.Take(i * 8, 8));

                if (entry.Type == 1)
                {
                    var length = stringStream.Value.Value.Take(entry.PropertyId, 4).ToInt32();

                    var value = stringStream.Value.Value.Take(entry.PropertyId + 4, length);

                    var name = Encoding.Unicode.GetString(value);

                    if (name == propertyName)
                    {
                        return Maybe<PropertyId>.OfValue(new PropertyId(entry.PropertyIndex + 0x8000));
                    }
                }
            }

            return Maybe<PropertyId>.NoValue();
        }
    }
}
