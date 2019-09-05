using Microsoft.ServiceFabric.Services.Remoting.V2;
using Microsoft.ServiceFabric.Services.Remoting.V2.Messaging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json.Serialization;

namespace Eshopworld.Web
{
    /// <inheritdoc />
    /// <summary>
    /// custom serialization provider - JSON based
    /// </summary>
    public class ServiceRemotingJsonSerializationProvider : IServiceRemotingMessageSerializationProvider
    {
        public IServiceRemotingMessageBodyFactory CreateMessageBodyFactory()
        {
            return new JsonMessageFactory();
        }

        public IServiceRemotingRequestMessageBodySerializer CreateRequestMessageSerializer(Type serviceInterfaceType, IEnumerable<Type> requestWrappedType, IEnumerable<Type> requestBodyTypes = null)
        {
            return new ServiceRemotingRequestJsonMessageBodySerializer();
        }

        public IServiceRemotingResponseMessageBodySerializer CreateResponseMessageSerializer(Type serviceInterfaceType, IEnumerable<Type> responseWrappedType, IEnumerable<Type> responseBodyTypes = null)
        {
            return new ServiceRemotingResponseJsonMessageBodySerializer();
        }
    }

    /// <inheritdoc />
    internal class JsonMessageFactory : IServiceRemotingMessageBodyFactory
    {

        public IServiceRemotingRequestMessageBody CreateRequest(string interfaceName, string methodName, int numberOfParameters, object wrappedRequestObject)
        {
            return new JsonBody(wrappedRequestObject);
        }

        public IServiceRemotingResponseMessageBody CreateResponse(string interfaceName, string methodName, object wrappedRequestObject)
        {
            return new JsonBody(wrappedRequestObject);
        }
    }

    /// <inheritdoc />
    internal class ServiceRemotingRequestJsonMessageBodySerializer : IServiceRemotingRequestMessageBodySerializer
    {
        private readonly JsonSerializer _serializer;

        public ServiceRemotingRequestJsonMessageBodySerializer()
        {
            _serializer = JsonSerializer.Create(new JsonSerializerSettings()
            {
                TypeNameHandling = TypeNameHandling.All,
                SerializationBinder = new CustomSerializationBinder()
            });
        }

        public IOutgoingMessageBody Serialize(IServiceRemotingRequestMessageBody serviceRemotingRequestMessageBody)
        {
            if (serviceRemotingRequestMessageBody == null)
            {
                return null;
            }
            
            using var writeStream = new MemoryStream();
            using var jsonWriter = new JsonTextWriter(new StreamWriter(writeStream));
            _serializer.Serialize(jsonWriter, serviceRemotingRequestMessageBody);
            jsonWriter.Flush();
            var bytes = writeStream.ToArray();
            var segment = new ArraySegment<byte>(bytes);
            var segments = new List<ArraySegment<byte>> { segment };
            return new OutgoingMessageBody(segments);
        }

        public IServiceRemotingRequestMessageBody Deserialize(IIncomingMessageBody messageBody)
        {
            using var sr = new StreamReader(messageBody.GetReceivedBuffer());
            using JsonReader reader = new JsonTextReader(sr);
            return _serializer.Deserialize<JsonBody>(reader);
        }
    }

    /// <inheritdoc />
    internal class ServiceRemotingResponseJsonMessageBodySerializer : IServiceRemotingResponseMessageBodySerializer
    {
        private readonly JsonSerializer _serializer;

        public ServiceRemotingResponseJsonMessageBodySerializer()
        {
            _serializer = JsonSerializer.Create(new JsonSerializerSettings()
            {
                TypeNameHandling = TypeNameHandling.All,
                SerializationBinder = new CustomSerializationBinder()
            });
        }

        public IOutgoingMessageBody Serialize(IServiceRemotingResponseMessageBody responseMessageBody)
        {
            if (responseMessageBody == null)
            {
                return null;
            }

            using var writeStream = new MemoryStream();
            using var jsonWriter = new JsonTextWriter(new StreamWriter(writeStream));
            _serializer.Serialize(jsonWriter, responseMessageBody);
            jsonWriter.Flush();
            var bytes = writeStream.ToArray();
            var segment = new ArraySegment<byte>(bytes);
            var segments = new List<ArraySegment<byte>> { segment };
            return new OutgoingMessageBody(segments);
        }

        public IServiceRemotingResponseMessageBody Deserialize(IIncomingMessageBody messageBody)
        {

            using var sr = new StreamReader(messageBody.GetReceivedBuffer());
            using var reader = new JsonTextReader(sr);
            return _serializer.Deserialize<JsonBody>(reader);
        }
    }

    /// wrapped JSON message - both request and response
    internal class JsonBody : WrappedMessage, IServiceRemotingRequestMessageBody, IServiceRemotingResponseMessageBody
    {
        public JsonBody(object wrapped)
        {
            Value = wrapped;
        }

        public void SetParameter(int position, string parameName, object parameter)
        {  //Not Needed if you are using WrappedMessage
            throw new NotImplementedException();
        }

        public object GetParameter(int position, string parameName, Type paramType)
        {
            //Not Needed if you are using WrappedMessage
            throw new NotImplementedException();
        }

        public void Set(object response)
        { //Not Needed if you are using WrappedMessage
            throw new NotImplementedException();
        }

        public object Get(Type paramType)
        {  //Not Needed if you are using WrappedMessage
            throw new NotImplementedException();
        }
    }

    /// <inheritdoc />
    internal class CustomSerializationBinder : DefaultSerializationBinder
    {
        public override void BindToName(Type serializedType, out string assemblyName, out string typeName)
        {
            typeName = serializedType.FullName;
            assemblyName = serializedType.Assembly.FullName;
        }

        public override Type BindToType(string assemblyName, string typeName)
        {
            Type type = null;
            if (assemblyName.Contains("_.service.mt"))
            {
                var assembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.FullName == assemblyName || a.GetName().Name == assemblyName);
                type = assembly?.GetType(typeName);
            }
            return type ?? base.BindToType(assemblyName, typeName);
        }
    }
}
