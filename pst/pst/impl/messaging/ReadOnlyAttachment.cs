﻿using pst.core;
using pst.encodables.ndb;
using pst.interfaces;
using pst.interfaces.messaging;

namespace pst.impl.messaging
{
    class ReadOnlyAttachment : IReadOnlyAttachment
    {
        private readonly IDecoder<NID> nidDecoder;
        private readonly INodeEntryFinder nodeEntryFinder;
        private readonly IPropertyContextBasedReadOnlyComponent readOnlyComponent;

        public ReadOnlyAttachment(
            IDecoder<NID> nidDecoder,
            INodeEntryFinder nodeEntryFinder,
            IPropertyContextBasedReadOnlyComponent readOnlyComponent)
        {
            this.nidDecoder = nidDecoder;
            this.nodeEntryFinder = nodeEntryFinder;
            this.readOnlyComponent = readOnlyComponent;
        }

        public Maybe<NID> GetEmbeddedMessageNodeId(NID[] attachmentNodePath)
        {
            var entry = nodeEntryFinder.GetEntry(attachmentNodePath);

            if (entry.HasNoValue)
            {
                return Maybe<NID>.NoValue();
            }

            var attachMethodPropertyValue =
                readOnlyComponent.GetProperty(attachmentNodePath, MAPIProperties.PidTagAttachMethod);

            if (attachMethodPropertyValue.HasNoValue ||
               !attachMethodPropertyValue.Value.Value.HasFlag(MAPIProperties.afEmbeddedMessage))
            {
                return Maybe<NID>.NoValue();
            }

            var attachDataObject =
                readOnlyComponent.GetProperty(attachmentNodePath, MAPIProperties.PidTagAttachDataObject);

            return nidDecoder.Decode(attachDataObject.Value.Value.Take(4));
        }
    }
}