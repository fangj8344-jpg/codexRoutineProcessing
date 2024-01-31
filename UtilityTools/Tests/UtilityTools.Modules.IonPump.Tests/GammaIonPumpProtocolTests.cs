using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UtilityTools.Core.Helper;
using UtilityTools.Modules.IonPump.Protocol;

namespace UtilityTools.Modules.IonPump.Tests
{
    public class GammaIonPumpProtocolTests
    {

        [Test]
        public void TestCommand()
        {
            var builder = new CommandPacketBuilder();
            byte[] bytes = builder.GetCmdSysModel().GetBytes();
            Console.WriteLine(Encoding.ASCII.GetString(bytes));
            Console.WriteLine(StrHelper.HexStr(bytes));

            bytes = builder.GetCmdHvReadPressure().GetBytes();
            Console.WriteLine(Encoding.ASCII.GetString(bytes));
            Console.WriteLine(StrHelper.HexStr(bytes));

            bytes = builder.GetCmdHvReadVoltage().GetBytes();
            Console.WriteLine(Encoding.ASCII.GetString(bytes));
            Console.WriteLine(StrHelper.HexStr(bytes));

            bytes = builder.GetReadCmd(EnumCommand.VERSION).GetBytes();
            Console.WriteLine(Encoding.ASCII.GetString(bytes));
            Console.WriteLine(StrHelper.HexStr(bytes));

            bytes = builder.GetReadCmd(EnumCommand.GET_SET_SERIAL_COMM).GetBytes();
            Console.WriteLine(Encoding.ASCII.GetString(bytes));
            Console.WriteLine(StrHelper.HexStr(bytes));
        }


        [Test]
        public void TestParser()
        {
            var builder = new CommandPacketBuilder();
            var parser = new ResponsePacketParser();
            List<ResponsePacket> packets = new List<ResponsePacket>();
            parser.PacketReceivedEvent += (object? sender, ResponsePacket packet) => { packets.Add(packet); };

            //byte[] bytes = builder.GetCmdSysModel().GetBytes();
            //byte[] bytes = Encoding.ASCII.GetBytes("05 OK 00 SPC2 F7\r");
            parser.ReceiveBytes(Encoding.ASCII.GetBytes("Helloworld"));
            parser.ReceiveBytes(Encoding.ASCII.GetBytes("05 OK 00 SPC2 F7\r"));

            byte[] bytes = Encoding.ASCII.GetBytes("05 OK 00 FIRMWARE 1.18.12 B5\r05 OK 00 FIRMWARE");
            parser.ReceiveBytes(bytes);

            bytes = Encoding.ASCII.GetBytes(" 1.18.12 B5\r");
            parser.ReceiveBytes(bytes);
        }
    }
}
