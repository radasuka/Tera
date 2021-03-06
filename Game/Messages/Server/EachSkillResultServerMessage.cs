﻿using System;
using System.Diagnostics;

namespace Tera.Game.Messages
{
    public class EachSkillResultServerMessage : ParsedMessage
    {
        [Flags]
        public enum SkillResultFlags
        {
            Bit0 = 1, // Usually 1 for attacks, 0 for blocks/dodges but I don't understand its exact semantics yet
            Heal = 2, // Bit0 == 1 + heal == 1 = mana
            Bit2 = 4,
            IsDfaResolve = 4,
            Bit16 = 0x10000,
            Bit18 = 0x40000
        }

        internal EachSkillResultServerMessage(TeraMessageReader reader)
            : base(reader)
        {
            reader.Skip(4);
            Source = reader.ReadEntityId();
            reader.Skip(8);
            Target = reader.ReadEntityId();
            Unknow1 = reader.ReadBytes(4);//templateid

            SkillId = new SkillId(reader).Id;

            //Not sure if it s a int32. or int16 or int64 or other thing 
            //When using a skill with many hit, each hit seem to have a different number (ex: 0, 1, 2, or 3)
            HitId = reader.ReadInt32(); //stage
            Unknow2 = reader.ReadBytes(12); //index in targeting list, index in area, id, time

            Amount = reader.Factory.ReleaseVersion < 6200 ? reader.ReadInt32() : reader.ReadInt64();// KR now use 64 bit
            FlagsDebug = reader.ReadInt32();
            Flags = (SkillResultFlags) FlagsDebug;
            IsCritical = (reader.ReadByte() & 1) != 0;
            ConsumeEdge = (reader.ReadByte() & 1) != 0;
            Blocked = (reader.ReadByte() & 1) != 0;
//            reader.Skip(reader.Factory.ReleaseVersion>=7401 ? 8 : 12); //KR removed something
            reader.Skip(12); //KR added something back
            Position = reader.ReadVector3f();
            Heading = reader.ReadAngle();
            //if (Position.X!=0)
            //    Debug.WriteLine($"{Time.Ticks} {BitConverter.ToString(BitConverter.GetBytes(Target.Id))} {SkillId} {Position} {Heading}");
        }

        //DEBUG
        public int FlagsDebug { get; }


        public int HitId { get; }

        //DEBUG
        public byte[] Unknow1 { get; }

        //DEBUG
        public byte[] Unknow2 { get; }


        public EntityId Source { get; private set; }
        public EntityId Target { get; }
        public long Amount { get; }
        public int SkillId { get; private set; }
        public SkillResultFlags Flags { get; }
        public bool IsCritical { get; private set; }
        public bool ConsumeEdge { get; private set; }
        public bool Blocked { get; private set; }
        public Vector3f Position { get; }
        public Angle Heading { get; }

        public bool IsMana => ((Flags & SkillResultFlags.Bit0) != 0) && ((Flags & SkillResultFlags.Heal) != 0);

        public bool IsHeal => ((Flags & SkillResultFlags.Bit0) == 0) && ((Flags & SkillResultFlags.Heal) != 0);
        public bool IsUseless => (Flags & SkillResultFlags.IsDfaResolve) != 0;
        public bool IsHp => !IsMana;
    }
}