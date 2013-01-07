﻿// ------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     ImmutableTree Version: 0.0.0.1
//  
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
// ------------------------------------------------------------------------------


public partial class Basket {

	private readonly System.Int32 size;

	private readonly System.Collections.Immutable.ImmutableList<Fruit> contents;

	/// <summary>Initializes a new instance of the Basket class.</summary>
	public Basket()
	{
	}

	/// <summary>Initializes a new instance of the Basket class.</summary>
	public Basket(System.Int32 size, System.Collections.Immutable.ImmutableList<Fruit> contents)
	{
		this.size = size;
		this.contents = contents;
	}

	public System.Int32 Size {
		get { return this.size; }
	}

	public Basket WithSize(System.Int32 value) {
		return new Basket(value, this.Contents);
	}

	public System.Collections.Immutable.ImmutableList<Fruit> Contents {
		get { return this.contents; }
	}

	public Basket WithContents(System.Collections.Immutable.ImmutableList<Fruit> value) {
		return new Basket(this.Size, value);
	}
}

public partial class Fruit {

	private readonly System.String color;

	private readonly System.Int32 skinThickness;

	/// <summary>Initializes a new instance of the Fruit class.</summary>
	public Fruit()
	{
	}

	/// <summary>Initializes a new instance of the Fruit class.</summary>
	public Fruit(System.String color, System.Int32 skinThickness)
	{
		this.color = color;
		this.skinThickness = skinThickness;
	}

	public System.String Color {
		get { return this.color; }
	}

	public Fruit WithColor(System.String value) {
		return new Fruit(value, this.SkinThickness);
	}

	public System.Int32 SkinThickness {
		get { return this.skinThickness; }
	}

	public Fruit WithSkinThickness(System.Int32 value) {
		return new Fruit(this.Color, value);
	}
}
