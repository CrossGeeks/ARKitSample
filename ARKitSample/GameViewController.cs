using System;
using SceneKit;
using UIKit;
using ARKit;
using CoreGraphics;
using Foundation;
using CoreAnimation;
using System.Collections.Generic;

namespace ARKitSample
{
    public enum BitMaskCategory{
        Pokeball = 2,
        Pokemon  = 3
    }
	public partial class GameViewController : UIViewController, IARSCNViewDelegate
	{
      
		#region Computed Properties
		public ARSCNView SceneView 
        {
			get { return View as ARSCNView; }
		}
		#endregion

		#region Constructors
		protected GameViewController(IntPtr handle) : base(handle)
		{
			// Note: this .ctor should not contain any initialization logic.
		}
		#endregion

		#region Override Methods
		public override void ViewDidLoad()
		{
			base.ViewDidLoad();

			// Set self as the Scene Vi.pew's delegate
			SceneView.Delegate = this;

			// Track changes to the session
			SceneView.Session.Delegate = new SessionDelegate();
            SceneView.Scene.PhysicsWorld.ContactDelegate = new PhysicsDelegate();

            SceneView.AutomaticallyUpdatesLighting = true;
		
		}

		public override void ViewWillAppear(bool animated)
		{
			base.ViewWillAppear(animated);

			// Create a session configuration
			var configuration = new ARWorldTrackingConfiguration {
				PlaneDetection = ARPlaneDetection.Horizontal,
				LightEstimationEnabled = true
			};
            SceneView.DebugOptions = ARSCNDebugOptions.ShowFeaturePoints | ARSCNDebugOptions.ShowWorldOrigin;
			// Run the view's session
			SceneView.Session.Run(configuration, ARSessionRunOptions.ResetTracking);

            SceneView.Scene.RootNode.AddChildNode(CreatePokemonNodeFromFile("art.scnassets/Charizard.scn", "charizard", new SCNVector3(2f, -2f, -9f)));
            SceneView.Scene.RootNode.AddChildNode(CreatePokemonNodeFromFile("art.scnassets/Pikachu.scn", "pikachu", new SCNVector3(8f, -1f, -6f)));
            SceneView.Scene.RootNode.AddChildNode(CreatePokemonNodeFromFile("art.scnassets/Bulbasaur.scn", "bulbasaur", new SCNVector3(2f, -2f, 9f)));
            SceneView.Scene.RootNode.AddChildNode(CreatePokemonNodeFromFile("art.scnassets/Scyther.scn", "scyther", new SCNVector3(8f, -1f, 6f)));
        }

        SCNNode CreatePokemonNodeFromFile(string filePath,string nodeName,SCNVector3 vector)
        {
            var pScene = SCNScene.FromFile(filePath);
            var pokemon = pScene.RootNode.FindChildNode(nodeName, true);
            pokemon.Position = vector;
            pokemon.PhysicsBody = SCNPhysicsBody.CreateStaticBody();
            pokemon.PhysicsBody.PhysicsShape = SCNPhysicsShape.Create(pokemon);
            pokemon.PhysicsBody.ContactTestBitMask = (int)BitMaskCategory.Pokeball;
            pokemon.PhysicsBody.CategoryBitMask = (int)BitMaskCategory.Pokemon;
            return pokemon;
        }

		public override void ViewWillDisappear(bool animated)
		{
			base.ViewWillDisappear(animated);

			// Pause the view's session
			SceneView.Session.Pause();
		}

		public override bool ShouldAutorotate()
		{
			return true;
		}

		public override UIInterfaceOrientationMask GetSupportedInterfaceOrientations()
		{
			return UIInterfaceOrientationMask.All;
		}
		


        public override void TouchesBegan(NSSet touches, UIEvent evt)
        {
            var forcePower = 10;

            base.TouchesBegan(touches, evt);
            var pointOfView = this.SceneView.PointOfView;
            var transform = pointOfView.Transform;
            var location = new SCNVector3(transform.M41, transform.M42, transform.M43);
            var orientation = new SCNVector3(-transform.M31, -transform.M32, -transform.M33);
            var position = location + orientation;
            var pokeball = new SCNNode()
            {
                Geometry = SCNSphere.Create(0.15f),

            };
            pokeball.Geometry.FirstMaterial.Diffuse.ContentImage = UIImage.FromBundle("pokeball");
            pokeball.Position =position; 
            pokeball.PhysicsBody = SCNPhysicsBody.CreateDynamicBody();
            pokeball.PhysicsBody.PhysicsShape = SCNPhysicsShape.Create(pokeball);
            pokeball.PhysicsBody.ContactTestBitMask = (int) BitMaskCategory.Pokemon;
            pokeball.PhysicsBody.CategoryBitMask = (int)BitMaskCategory.Pokeball;

            pokeball.PhysicsBody.ApplyForce(new SCNVector3(orientation.X*forcePower,orientation.Y*forcePower,orientation.Z*forcePower),true);
            SceneView.Scene.RootNode.AddChildNode(pokeball);
        }

      

        #endregion


        public class PhysicsDelegate : SCNPhysicsContactDelegate
        {
            IList<SCNNode> animatedNodes=new List<SCNNode>(); 

            public override void DidBeginContact(SCNPhysicsWorld world, SCNPhysicsContact contact)
            {

                var nodeA = contact.NodeA;
                var nodeB = contact.NodeB;
                SCNNode target = null;
                if (nodeA.PhysicsBody.CategoryBitMask == (int)BitMaskCategory.Pokemon)
                {
                    target = nodeA;
                }
                else if (nodeB.PhysicsBody.CategoryBitMask == (int)BitMaskCategory.Pokemon)
                {
                    target = nodeB;
                }

                if (target != null && !animatedNodes.Contains(target))
                {
                    var targetScale=target.Scale;

                    animatedNodes.Add(target);

                    target.RunAction(SCNAction.Sequence(new SCNAction[]{
                        SCNAction.ScaleTo(0, 1.0f),
                        SCNAction.ScaleTo(targetScale.X, 2.0f),
                    }),()=>{
                        animatedNodes.Remove(target);
                    });
                 
                }
               
            }


        }

      
    }
}
