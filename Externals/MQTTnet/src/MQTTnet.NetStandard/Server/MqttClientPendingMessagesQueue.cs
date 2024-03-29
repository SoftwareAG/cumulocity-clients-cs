﻿using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using MQTTnet.Adapter;
using MQTTnet.Diagnostics;
using MQTTnet.Exceptions;
using MQTTnet.Packets;
using MQTTnet.Protocol;

namespace MQTTnet.Server
{
    public sealed class MqttClientPendingMessagesQueue : IDisposable
    {
        private readonly ConcurrentQueue<MqttBasePacket> _queue = new ConcurrentQueue<MqttBasePacket>();
        private readonly SemaphoreSlim _queueWaitSemaphore = new SemaphoreSlim(0);
        private readonly IMqttServerOptions _options;
        private readonly MqttClientSession _clientSession;
        private readonly IMqttNetLogger _logger;

        public MqttClientPendingMessagesQueue(IMqttServerOptions options, MqttClientSession clientSession, IMqttNetLogger logger)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _clientSession = clientSession ?? throw new ArgumentNullException(nameof(clientSession));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public int Count => _queue.Count;

        public void Start(IMqttChannelAdapter adapter, CancellationToken cancellationToken)
        {
            if (adapter == null) throw new ArgumentNullException(nameof(adapter));

            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }

            Task.Run(async () => await SendQueuedPacketsAsync(adapter, cancellationToken), cancellationToken).ConfigureAwait(false);
        }

        public void Enqueue(MqttBasePacket packet)
        {
            if (packet == null) throw new ArgumentNullException(nameof(packet));

            _queue.Enqueue(packet);
            _queueWaitSemaphore.Release();

            _logger.Trace<MqttClientPendingMessagesQueue>("Enqueued packet (ClientId: {0}).", _clientSession.ClientId);
        }

        private async Task SendQueuedPacketsAsync(IMqttChannelAdapter adapter, CancellationToken cancellationToken)
        {
            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    await SendQueuedPacketAsync(adapter, cancellationToken);
                }
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception exception)
            {
                _logger.Error<MqttClientPendingMessagesQueue>(exception, "Unhandled exception while sending enqueued packet (ClientId: {0}).", _clientSession.ClientId);
            }
        }

        private async Task SendQueuedPacketAsync(IMqttChannelAdapter adapter, CancellationToken cancellationToken)
        {
            MqttBasePacket packet = null;
            try
            {
                await _queueWaitSemaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
                if (!_queue.TryDequeue(out packet))
                {
                    throw new InvalidOperationException(); // should not happen
                }

                await adapter.SendPacketsAsync(_options.DefaultCommunicationTimeout, cancellationToken, packet).ConfigureAwait(false);

                _logger.Trace<MqttClientPendingMessagesQueue>("Enqueued packet sent (ClientId: {0}).", _clientSession.ClientId);
            }
            catch (Exception exception)
            {
                if (exception is MqttCommunicationTimedOutException)
                {
                    _logger.Warning<MqttClientPendingMessagesQueue>(exception, "Sending publish packet failed due to timeout (ClientId: {0}).", _clientSession.ClientId);
                }
                else if (exception is MqttCommunicationException)
                {
                    _logger.Warning<MqttClientPendingMessagesQueue>(exception, "Sending publish packet failed due to communication exception (ClientId: {0}).", _clientSession.ClientId);
                }
                else if (exception is OperationCanceledException)
                {
                }
                else
                {
                    _logger.Error<MqttClientPendingMessagesQueue>(exception, "Sending publish packet failed (ClientId: {0}).", _clientSession.ClientId);
                }

                if (packet is MqttPublishPacket publishPacket)
                {
                    if (publishPacket.QualityOfServiceLevel > MqttQualityOfServiceLevel.AtMostOnce)
                    {
                        publishPacket.Dup = true;
                        _queue.Enqueue(packet);
                        _queueWaitSemaphore.Release();
                    }
                }

                await _clientSession.StopAsync();
            }
        }

        public void Dispose()
        {
            _queueWaitSemaphore?.Dispose();
        }
    }
}
