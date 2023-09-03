using Raylib_cs;
using System.Numerics;
using ChessBot.Helpers;

namespace ChessBot.Application;

public class StackRenderer : ScreenObject {
	public int padding = 6;
	// Need a queue to avoid "System.InvalidOperationException: Collection was modified after the enumerator was instantiated."
	private Queue<ScreenObject> elementsToAdd = new Queue<ScreenObject>();
	private Queue<ScreenObject> elementsToRemove = new Queue<ScreenObject>();
	public List<ScreenObject> children = new List<ScreenObject>();
	public StackRenderer(Vector2 position, params ScreenObject[] elements) : base(new Rectangle(position.X, position.Y, 0, 0)) {
		AddElements(elements);
		color = BoardTheme.borderCol;
		LeftPressed = delegate{};	// Nullify mouse behavior
		LeftReleased = delegate{};	// Nullify mouse behavior
		RightPressed = delegate{};	// Nullify mouse behavior
		RightReleased = delegate{};	// Nullify mouse behavior
	}
	public void AddElement(ScreenObject element) { elementsToAdd.Enqueue(element); RecalculateChildrenPositions(); }
	public void AddElements(params ScreenObject[] elements) { foreach (ScreenObject element in elements) { elementsToAdd.Enqueue(element); RecalculateChildrenPositions(); } }
	public void RemoveElement(System.Index index) { elementsToRemove.Enqueue(children[index]); RecalculateChildrenPositions(); }
	public void RemoveElement(ScreenObject element) { elementsToRemove.Enqueue(element); RecalculateChildrenPositions(); }
	public void RemoveElements(params ScreenObject[] elements) { foreach (ScreenObject element in elements) { elementsToRemove.Enqueue(element); RecalculateChildrenPositions(); } }

	public override void Update(float dt) {
		foreach (ScreenObject child in children) { child.Update(); }



		bool shouldUpdate = false;
		if (elementsToAdd.Count + elementsToRemove.Count != 0) shouldUpdate = true;
		while (elementsToAdd.Count != 0) { children.Add(elementsToAdd.Dequeue()); }
		while (elementsToRemove.Count != 0) { children.Remove(elementsToRemove.Dequeue()); }
		if (shouldUpdate) RecalculateChildrenPositions();
	}

	public override void Draw(Vector2 pos) {
		UIHelper.DrawRectangleAsBorder(_Rect, color, padding);
		foreach (ScreenObject child in children) { child.Draw(); }
	}

	public void RecalculateChildrenPositions() {
		// TODO: fix buttons of different widths not being centered

		float totalHeight = 0.0f;
		float maxWidth = 0.0f;
		// Need to calaculate total Size before iterating on children because `CENTER` depends on value of `Size`
		foreach (ScreenObject child in children) {
			totalHeight += child.Size.Y+padding;
			maxWidth = Math.Max(maxWidth, child.Size.X);
		}
		totalHeight -= padding; // remove padding value

		Size = new Vector2(maxWidth, totalHeight);
		Vector2 iterDelta = new Vector2();
		foreach (ScreenObject child in children.ToArray().Reverse()) {
			child.Position = TOPLEFT;
			iterDelta = new Vector2(0, child.Size.Y+padding);


			foreach (ScreenObject subChild in children.ToArray().Reverse()) {
				if (subChild == child) break;
				// Loop though all children from beginning to current child
				subChild.Position += iterDelta;
			}
		}

		// // Iterate one last time to adjust positions based on total height
		// foreach (ScreenObject child in children) {
		// 	child.Position -= ;
		// }


	}

}