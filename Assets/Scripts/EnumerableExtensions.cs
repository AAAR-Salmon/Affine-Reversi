using System.Linq;
using System.Collections.Generic;
using System;

static class EnumerableExtensions {
	public static TSource RandomElement<TSource>(this IEnumerable<TSource> source) {
		Random rnd = new Random();
		TSource[] sourceArr = source.ToArray();
		return sourceArr[rnd.Next(sourceArr.Length)];
	}

	public static TSource RandomElement<TSource>(this IList<TSource> source) {
		Random rnd = new Random();
		return source[rnd.Next(source.Count)];
	}

	public static T ChooseRandom<T>(params T[] arr) {
		return arr.RandomElement();
	}
}