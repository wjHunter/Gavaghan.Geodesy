/* Gavaghan.Geodesy by Mike Gavaghan
 * 
 * http://www.gavaghan.org/blog/free-source-code/geodesy-library-vincentys-formula/
 * 
 * This code may be freely used and modified on any personal or professional
 * project.  It comes with no warranty.
 *
 * BitCoin tips graciously accepted at 1FB63FYQMy7hpC2ANVhZ5mSgAZEtY1aVLf
 */
using System;
using System.Globalization;
using System.Runtime.Serialization;

namespace Gavaghan.Geodesy
{
    /// <summary>
    /// Encapsulation of latitude and longitude coordinates on a globe.  Negative
    /// latitude is southern hemisphere.  Negative longitude is western hemisphere.
    /// 
    /// Any angle may be specified for longtiude and latitude, but all angles will
    /// be canonicalized such that:
    /// 
    ///      -90 &lt;= latitude &lt;= +90
    ///     -180 &lt;  longitude &lt;= +180
    /// </summary>
    [Serializable]
    public struct GlobalCoordinates : IComparable<GlobalCoordinates>, IComparable, IEquatable<GlobalCoordinates>, ISerializable
    {
        private const double PiOver2 = Math.PI / 2;
        private const double TwoPi = Math.PI + Math.PI;
        private const double NegativePiOver2 = -PiOver2;
        private const double NegativeTwoPi = -TwoPi;

        // intentionally NOT readonly, for performance reasons.
        /// <summary>Latitude.  Negative latitude is southern hemisphere.</summary>
        private Angle latitude;

        // intentionally NOT readonly, for performance reasons.
        /// <summary>Longitude.  Negative longitude is western hemisphere.</summary>
        private Angle longitude;

        /// <summary>
        /// Construct a new GlobalCoordinates.  Angles will be canonicalized.
        /// </summary>
        /// <param name="latitude">latitude</param>
        /// <param name="longitude">longitude</param>
        public GlobalCoordinates(Angle latitude, Angle longitude)
        {
            this.latitude = latitude;
            this.longitude = longitude;
            this.Canonicalize();
        }

        /// <summary>
        /// Get latitude.  The latitude value will be canonicalized (which might
        /// result in a change to the longitude). Negative latitude is southern hemisphere.
        /// </summary>
        public Angle Latitude
        {
            get { return latitude; }
        }

        /// <summary>
        /// Get longitude.  The longitude value will be canonicalized. Negative
        /// longitude is western hemisphere.
        /// </summary>
        public Angle Longitude
        {
            get { return longitude; }
        }

        /// <summary>
        /// Canonicalize the current latitude and longitude values such that:
        /// 
        ///      -90 &lt;= latitude  &lt;=  +90
        ///     -180 &lt;  longitude &lt;= +180
        /// </summary>
        private void Canonicalize()
        {
            // To understand why this works the way it does, imagine walking along a meridian,
            // starting at the South Pole, heading north.  As you keep going north, your latitude
            // gets bigger and bigger, until you reach the North Pole.  You've now walked from -90
            // to 90, and mathematically, that's as high as latitude can go.  However, you're
            // completely capable of continuing to walk in that same straight line.  A little
            // farther (relatively speaking), and you've walked 181 degrees of latitude, but now
            // you're walking on the opposite meridian.
            double latitudeRadians = this.latitude.Radians;
            double longitudeRadians = this.longitude.Radians;

            latitudeRadians = (latitudeRadians + Math.PI) % TwoPi;
            if (latitudeRadians < 0) latitudeRadians += TwoPi;
            latitudeRadians -= Math.PI;

            if (latitudeRadians > PiOver2)
            {
                latitudeRadians = Math.PI - latitudeRadians;
                longitudeRadians += Math.PI;
            }
            else if (latitudeRadians < NegativePiOver2)
            {
                latitudeRadians = -Math.PI - latitudeRadians;
                longitudeRadians += Math.PI;
            }

            longitudeRadians = ((longitudeRadians + Math.PI) % TwoPi);
            if (longitudeRadians <= 0) longitudeRadians += TwoPi;
            longitudeRadians -= Math.PI;

            this.latitude = Angle.FromRadians(latitudeRadians);
            this.longitude = Angle.FromRadians(longitudeRadians);
        }

        /// <summary>
        /// Compare these coordinates to another set of coordiates.  Western
        /// longitudes are less than eastern logitudes.  If longitudes are equal,
        /// then southern latitudes are less than northern latitudes.
        /// </summary>
        /// <param name="other">instance to compare to</param>
        /// <returns>-1, 0, or +1 as per IComparable contract</returns>
        public int CompareTo(object obj)
        {
            if (!(obj is GlobalCoordinates))
            {
                throw new ArgumentException("Can only compare GlobalCoordinates with other GlobalCoordinates.", "obj");
            }

            return this.CompareTo((GlobalCoordinates)obj);
        }

        /// <summary>
        /// Compare these coordinates to another set of coordiates.  Western
        /// longitudes are less than eastern logitudes.  If longitudes are equal,
        /// then southern latitudes are less than northern latitudes.
        /// </summary>
        /// <param name="other">instance to compare to</param>
        /// <returns>-1, 0, or +1 as per IComparable contract</returns>
        public int CompareTo(GlobalCoordinates other)
        {
            int a = this.longitude.CompareTo(other.longitude);

            return a == 0
                ? this.latitude.CompareTo(other.latitude)
                : a;
        }

        /// <summary>
        /// Get a hash code for these coordinates.
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            int hashCode = 17;

            hashCode = hashCode * 31 + this.longitude.GetHashCode();
            hashCode = hashCode * 31 + this.latitude.GetHashCode();

            return hashCode;
        }

        /// <summary>
        /// Compare these coordinates to another object for equality.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            return obj is GlobalCoordinates &&
                   this.Equals((GlobalCoordinates)obj);
        }

        /// <summary>
        /// Compare these coordinates to another object for equality.
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool Equals(GlobalCoordinates other)
        {
            return this.latitude == other.latitude &&
                   this.longitude == other.longitude;
        }

        /// <summary>
        /// Get coordinates as a string.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return String.Format(CultureInfo.InvariantCulture,
                                 "GlobalCoordinates[Longitude={0}, Latitude={1}]",
                                 this.longitude,
                                 this.latitude);
        }

        #region Serialization / Deserialization

        private GlobalCoordinates(SerializationInfo info, StreamingContext context)
        {
            double longitudeRadians = info.GetDouble("longitudeRadians");
            double latitudeRadians = info.GetDouble("latitudeRadians");

            this.longitude = Angle.FromRadians(longitudeRadians);
            this.latitude = Angle.FromRadians(latitudeRadians);
        }

        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("longitudeRadians", this.longitude.Radians);
            info.AddValue("latitudeRadians", this.latitude.Radians);
        }

        #endregion

        #region Operators

        public static bool operator ==(GlobalCoordinates lhs, GlobalCoordinates rhs)
        {
            return lhs.Equals(rhs);
        }

        public static bool operator !=(GlobalCoordinates lhs, GlobalCoordinates rhs)
        {
            return !lhs.Equals(rhs);
        }

        public static bool operator <(GlobalCoordinates lhs, GlobalCoordinates rhs)
        {
            return lhs.CompareTo(rhs) < 0;
        }

        public static bool operator <=(GlobalCoordinates lhs, GlobalCoordinates rhs)
        {
            return lhs.CompareTo(rhs) <= 0;
        }

        public static bool operator >(GlobalCoordinates lhs, GlobalCoordinates rhs)
        {
            return lhs.CompareTo(rhs) > 0;
        }

        public static bool operator >=(GlobalCoordinates lhs, GlobalCoordinates rhs)
        {
            return lhs.CompareTo(rhs) >= 0;
        }

        #endregion
    }
}
