﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GrahamBot
{
    public class Space : IEquatable<Space>
    {
        public Space() { }
        public Space(int x, int y)
        {
            this.x = x;
            this.y = y;
        }
        public int x { get; set; }
        public int y { get; set; }

        //public static EqualityComparer<Space> Default { get; }
        public bool Equals(Space other)
        {
            if (this.x == other.x & this.y == other.y)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public override bool Equals(object o)
        {
            var other = o as Space;
            return other == null ? false : (this.x == other.x & this.y == other.y);
        }
        public override int GetHashCode()
        {
            return this.x.GetHashCode() & this.y.GetHashCode();
        }
    }
}