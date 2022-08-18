using System;
using System.Globalization;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace XFlow.Modules.Grid.Model
{
  /// <summary>
  ///   <para>Representation of 2D vectors and points using integers.</para>
  /// </summary>
  public struct Vector2Int : IEquatable<Vector2Int>, IFormattable
  {
    public int m_X;
    public int m_Y;
    private static readonly Vector2Int s_Zero = new Vector2Int(0, 0);
    private static readonly Vector2Int s_One = new Vector2Int(1, 1);
    private static readonly Vector2Int s_Up = new Vector2Int(0, 1);
    private static readonly Vector2Int s_Down = new Vector2Int(0, -1);
    private static readonly Vector2Int s_Left = new Vector2Int(-1, 0);
    private static readonly Vector2Int s_Right = new Vector2Int(1, 0);

    /// <summary>
    ///   <para>X component of the vector.</para>
    /// </summary>
    public int x
    {
      [MethodImpl(MethodImplOptions.AggressiveInlining)] get => this.m_X;
      [MethodImpl(MethodImplOptions.AggressiveInlining)] set => this.m_X = value;
    }

    /// <summary>
    ///   <para>Y component of the vector.</para>
    /// </summary>
    public int y
    {
      [MethodImpl(MethodImplOptions.AggressiveInlining)] get => this.m_Y;
      [MethodImpl(MethodImplOptions.AggressiveInlining)] set => this.m_Y = value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Vector2Int(int x, int y)
    {
      this.m_X = x;
      this.m_Y = y;
    }

    /// <summary>
    ///   <para>Set x and y components of an existing Vector2Int.</para>
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Set(int x, int y)
    {
      this.m_X = x;
      this.m_Y = y;
    }

    public int this[int index]
    {
      [MethodImpl(MethodImplOptions.AggressiveInlining)] get
      {
        switch (index)
        {
          case 0:
            return this.x;
          case 1:
            return this.y;
          default:
            throw new IndexOutOfRangeException(string.Format("Invalid Vector2Int index addressed: {0}!", (object) index));
        }
      }
      [MethodImpl(MethodImplOptions.AggressiveInlining)] set
      {
        switch (index)
        {
          case 0:
            this.x = value;
            break;
          case 1:
            this.y = value;
            break;
          default:
            throw new IndexOutOfRangeException(string.Format("Invalid Vector2Int index addressed: {0}!", (object) index));
        }
      }
    }

    /// <summary>
    ///   <para>Returns the length of this vector (Read Only).</para>
    /// </summary>
    public float magnitude
    {
      [MethodImpl(MethodImplOptions.AggressiveInlining)] get => Mathf.Sqrt((float) (this.x * this.x + this.y * this.y));
    }

    /// <summary>
    ///   <para>Returns the squared length of this vector (Read Only).</para>
    /// </summary>
    public int sqrMagnitude
    {
      [MethodImpl(MethodImplOptions.AggressiveInlining)] get => this.x * this.x + this.y * this.y;
    }

    /// <summary>
    ///   <para>Returns the distance between a and b.</para>
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Distance(Vector2Int a, Vector2Int b)
    {
      float num1 = (float) (a.x - b.x);
      float num2 = (float) (a.y - b.y);
      return (float) Math.Sqrt((double) num1 * (double) num1 + (double) num2 * (double) num2);
    }

    /// <summary>
    ///   <para>Returns a vector that is made from the smallest components of two vectors.</para>
    /// </summary>
    /// <param name="lhs"></param>
    /// <param name="rhs"></param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2Int Min(Vector2Int lhs, Vector2Int rhs) => new Vector2Int(Mathf.Min(lhs.x, rhs.x), Mathf.Min(lhs.y, rhs.y));

    /// <summary>
    ///   <para>Returns a vector that is made from the largest components of two vectors.</para>
    /// </summary>
    /// <param name="lhs"></param>
    /// <param name="rhs"></param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2Int Max(Vector2Int lhs, Vector2Int rhs) => new Vector2Int(Mathf.Max(lhs.x, rhs.x), Mathf.Max(lhs.y, rhs.y));

    /// <summary>
    ///   <para>Multiplies two vectors component-wise.</para>
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2Int Scale(Vector2Int a, Vector2Int b) => new Vector2Int(a.x * b.x, a.y * b.y);

    /// <summary>
    ///   <para>Multiplies every component of this vector by the same component of scale.</para>
    /// </summary>
    /// <param name="scale"></param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Scale(Vector2Int scale)
    {
      this.x *= scale.x;
      this.y *= scale.y;
    }

    /// <summary>
    ///   <para>Clamps the Vector2Int to the bounds given by min and max.</para>
    /// </summary>
    /// <param name="min"></param>
    /// <param name="max"></param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Clamp(Vector2Int min, Vector2Int max)
    {
      this.x = Math.Max(min.x, this.x);
      this.x = Math.Min(max.x, this.x);
      this.y = Math.Max(min.y, this.y);
      this.y = Math.Min(max.y, this.y);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Vector2(Vector2Int v) => new Vector2((float) v.x, (float) v.y);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static explicit operator Vector3Int(Vector2Int v) => new Vector3Int(v.x, v.y, 0);

    /// <summary>
    ///   <para>Converts a Vector2 to a Vector2Int by doing a Floor to each value.</para>
    /// </summary>
    /// <param name="v"></param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2Int FloorToInt(Vector2 v) => new Vector2Int(Mathf.FloorToInt(v.x), Mathf.FloorToInt(v.y));

    /// <summary>
    ///   <para>Converts a  Vector2 to a Vector2Int by doing a Ceiling to each value.</para>
    /// </summary>
    /// <param name="v"></param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2Int CeilToInt(Vector2 v) => new Vector2Int(Mathf.CeilToInt(v.x), Mathf.CeilToInt(v.y));

    /// <summary>
    ///   <para>Converts a  Vector2 to a Vector2Int by doing a Round to each value.</para>
    /// </summary>
    /// <param name="v"></param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2Int RoundToInt(Vector2 v) => new Vector2Int(Mathf.RoundToInt(v.x), Mathf.RoundToInt(v.y));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2Int operator -(Vector2Int v) => new Vector2Int(-v.x, -v.y);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2Int operator +(Vector2Int a, Vector2Int b) => new Vector2Int(a.x + b.x, a.y + b.y);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2Int operator -(Vector2Int a, Vector2Int b) => new Vector2Int(a.x - b.x, a.y - b.y);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2Int operator *(Vector2Int a, Vector2Int b) => new Vector2Int(a.x * b.x, a.y * b.y);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2Int operator *(int a, Vector2Int b) => new Vector2Int(a * b.x, a * b.y);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2Int operator *(Vector2Int a, int b) => new Vector2Int(a.x * b, a.y * b);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2Int operator /(Vector2Int a, int b) => new Vector2Int(a.x / b, a.y / b);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator ==(Vector2Int lhs, Vector2Int rhs) => lhs.x == rhs.x && lhs.y == rhs.y;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator !=(Vector2Int lhs, Vector2Int rhs) => !(lhs == rhs);

    /// <summary>
    ///   <para>Returns true if the objects are equal.</para>
    /// </summary>
    /// <param name="other"></param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override bool Equals(object other) => other is Vector2Int other1 && Equals(other1);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Equals(Vector2Int other) => m_X == other.m_X && m_Y == other.m_Y;

    /// <summary>
    ///   <para>Gets the hash code for the Vector2Int.</para>
    /// </summary>
    /// <returns>
    ///   <para>The hash code of the Vector2Int.</para>
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override int GetHashCode() => m_X ^ (m_Y << 2);
    

    /// <summary>
    ///   <para>Returns a formatted string for this vector.</para>
    /// </summary>
    /// <param name="format">A numeric format string.</param>
    /// <param name="formatProvider">An object that specifies culture-specific formatting.</param>
    public override string ToString() => ToString(null, null);

    /// <summary>
    ///   <para>Returns a formatted string for this vector.</para>
    /// </summary>
    /// <param name="format">A numeric format string.</param>
    /// <param name="formatProvider">An object that specifies culture-specific formatting.</param>
    public string ToString(string format) => ToString(format, null);

    /// <summary>
    ///   <para>Returns a formatted string for this vector.</para>
    /// </summary>
    /// <param name="format">A numeric format string.</param>
    /// <param name="formatProvider">An object that specifies culture-specific formatting.</param>
    public string ToString(string format, IFormatProvider formatProvider)
    {
      formatProvider ??= CultureInfo.InvariantCulture.NumberFormat;
      return $"({x.ToString(format, formatProvider)}, {y.ToString(format, formatProvider)})";
    }

    /// <summary>
    ///   <para>Shorthand for writing Vector2Int(0, 0).</para>
    /// </summary>
    public static Vector2Int zero
    {
      [MethodImpl(MethodImplOptions.AggressiveInlining)] get => Vector2Int.s_Zero;
    }

    /// <summary>
    ///   <para>Shorthand for writing Vector2Int(1, 1).</para>
    /// </summary>
    public static Vector2Int one
    {
      [MethodImpl(MethodImplOptions.AggressiveInlining)] get => Vector2Int.s_One;
    }

    /// <summary>
    ///   <para>Shorthand for writing Vector2Int(0, 1).</para>
    /// </summary>
    public static Vector2Int up
    {
      [MethodImpl(MethodImplOptions.AggressiveInlining)] get => Vector2Int.s_Up;
    }

    /// <summary>
    ///   <para>Shorthand for writing Vector2Int(0, -1).</para>
    /// </summary>
    public static Vector2Int down
    {
      [MethodImpl(MethodImplOptions.AggressiveInlining)] get => Vector2Int.s_Down;
    }

    /// <summary>
    ///   <para>Shorthand for writing Vector2Int(-1, 0).</para>
    /// </summary>
    public static Vector2Int left
    {
      [MethodImpl(MethodImplOptions.AggressiveInlining)] get => Vector2Int.s_Left;
    }

    /// <summary>
    ///   <para>Shorthand for writing Vector2Int(1, 0).</para>
    /// </summary>
    public static Vector2Int right
    {
      [MethodImpl(MethodImplOptions.AggressiveInlining)] get => Vector2Int.s_Right;
    }
  }
}
