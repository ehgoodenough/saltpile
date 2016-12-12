﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PlayerManager : MonoBehaviour {
    private const float GRABDISTANCE = 2f;

    private Rigidbody2D body;
    private Vector2 cursorDirection;
    private GameObject highlightedBlock;
    public GameObject BlockCursorPrefab;
    private GameObject blockCursor;

    public float jumpSpeed = 20f;
    public float moveSpeed = 10f;
    public float throwSpeed = 20f;

    private const float NEGLIBILE_DISTANCE = 0.01f;
    private const float CAMERA_OFFCENTER = 3f;
    private const float WIN_HEIGHT = 40f;

    private GameObject held;

    private bool hasHitGround = false;

    void Start() {
        body = GetComponent<Rigidbody2D>();
        cursorDirection = Vector2.down;

        blockCursor = GameObject.Instantiate(BlockCursorPrefab, new Vector3(0, 0, 0), Quaternion.identity);
        blockCursor.SetActive(false);

        // If you're sick and tired of
        // the opening cutscene where
        // the player is thrown into
        // the prison, uncomment this.
        // if(Debug.isDebugBuild) {
        //     return;
        // }

        body.freezeRotation = false;
        transform.position = new Vector2(10f, 50f);
        body.AddForce(new Vector2(-15f, -15f), ForceMode2D.Impulse);
        body.AddTorque(-2f, ForceMode2D.Impulse);
    }

	void Update() {
        // Normalize the delta;
        float delta = Time.deltaTime / (1f / 60f);

        // If the player hasn't yet hit the ground
        // from being thrown into the prison...
        if(!hasHitGround) {
            Camera.main.transform.position = new Vector3(Camera.main.transform.position.x, transform.position.y, Camera.main.transform.position.z);

            if(Physics2D.CircleCast(transform.position, 0.25f, Vector2.down, 0.25f, LayerMask.GetMask("Block")).collider != null) {
                if(Input.anyKeyDown) {
                    transform.rotation = Quaternion.identity;
                    body.freezeRotation = true;
                    hasHitGround = true;
                }
            }

            // This will pre-maturely exit this method, so
            // none of the actual game logic will be run
            // during the opening cutscene where the player
            // is being thrown into the prison.
            return;
        }


        Vector2 targetVelocity = body.velocity;
        targetVelocity.x = Input.GetAxisRaw("Horizontal") * moveSpeed;

        body.velocity += ((targetVelocity - body.velocity) / 4) * delta;

        // If player has hit the jump key...
        if(Input.GetKey("w") || Input.GetKey("up")) {
            // ...And if the player is standing on the ground...
            if(Physics2D.CircleCast(transform.position, 0.25f, Vector2.down, 0.25f, LayerMask.GetMask("Block")).collider != null) {
                if(body.velocity.y <= 0) {
                    // ...Then jump!
                    body.AddForce(Vector2.up * jumpSpeed, ForceMode2D.Impulse);
                }
            }
        }

        Vector3 worldMouseLocation = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, Camera.main.transform.position.z * -1));
        worldMouseLocation.z = 0;
        cursorDirection = new Vector2(worldMouseLocation.x - transform.position.x, worldMouseLocation.y - (transform.position.y + transform.localScale.y / 2));

        if(held == null)
        {

        }
        else
        {
            held.transform.Translate(new Vector3(cursorDirection.normalized.x*2f, cursorDirection.normalized.y*2f, 0) + (transform.position - held.transform.position + new Vector3(0, transform.localScale.y, 0)));
            Vector3 cursorTargetPos = new Vector3(Mathf.Round(held.transform.position.x), (Mathf.Round(held.transform.position.y)), 0.00001f);
            blockCursor.transform.position += (cursorTargetPos - blockCursor.transform.position) / 2;
        }

        if (Input.GetMouseButtonDown(0) || Input.GetKeyDown("space")) {
            if(held == null) {
                if(highlightedBlock != null && highlightedBlock.name != "Ground") {
                    highlightedBlock.GetComponent<Rigidbody2D>().isKinematic = true;
                    highlightedBlock.GetComponent<BoxCollider2D>().enabled = false;

                    highlightedBlock.transform.parent = transform;
                    highlightedBlock.transform.localPosition = new Vector2(0, 3);
                    highlightedBlock.GetComponent<Rigidbody2D>().velocity = Vector2.zero;
                    highlightedBlock.GetComponent<Rigidbody2D>().angularVelocity = 0f;
                    highlightedBlock.transform.rotation = Quaternion.identity;

                    held = highlightedBlock;
                    blockCursor.SetActive(true);
                    held.GetComponent<SpriteRenderer>().enabled = false;                   
                    //unsetHighlightedBlock();
                }
            } else {
                held.GetComponent<BoxCollider2D>().enabled = true;
                held.GetComponent<Rigidbody2D>().isKinematic = (held.tag == "Rock");

                held.transform.parent = null;
                held.GetComponent<SpriteRenderer>().enabled = true;                

                if (held.tag == "Rock") {
                    // transform.Translate(new Vector2(0f, 0.5f));
                    // held.transform.localPosition = new Vector2(0f, 0f);

                    held.transform.position = new Vector3((blockCursor.transform.position.x), (Mathf.Round(blockCursor.transform.position.y)), 0f);
                } else {
                    held.GetComponent<Rigidbody2D>().AddForce(new Vector2(cursorDirection.x, cursorDirection.y)*throwSpeed);
                }


                held = null;
                blockCursor.SetActive(false);
            }
        }

        if(Physics2D.CircleCast(transform.position, 0.25f, Vector2.down, 0.25f, LayerMask.GetMask("Block")).collider != null) {
            Camera.main.transform.Translate(0f, ((transform.position.y + CAMERA_OFFCENTER) - Camera.main.transform.position.y) / 8f, 0f);
        }

        if(transform.position.y > WIN_HEIGHT) {
            GameObject message = GameObject.Find("Message");
            if(message != null) {
                message.GetComponent<Text>().text = "You Win!!";
            }
        }

        if(highlightedBlock != null)
        {
            Vector2 distVector = new Vector2(Mathf.Abs(highlightedBlock.transform.position.x - transform.position.x), Mathf.Abs(highlightedBlock.transform.position.y - (transform.position.y + transform.localScale.y / 2f)));
            if (distVector.magnitude > 2f)
            {
                unsetHighlightedBlock();
            }
        }      
    }

    void setNewHighlightedBlock(GameObject newBlock)
    {
        if (highlightedBlock != null)
        {
            unsetHighlightedBlock();
        }
        highlightedBlock = newBlock;
        highlightedBlock.GetComponent<SpriteRenderer>().color = new Color(.7f, .7f, .7f, 1f);
    }

    public void unsetHighlightedBlock()
    {
        if(highlightedBlock != null)
        {
            if(highlightedBlock.tag == "Rock")
            {
                highlightedBlock.GetComponent<SpriteRenderer>().color = new Color(0, 0, 0, 1);
            } else if(highlightedBlock.tag == "Dirt")
            {
                highlightedBlock.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 1);
            }
            highlightedBlock = null;
        }
    }

    public void tryToHighlight(GameObject block)
    {
        Vector2 distVector = new Vector2(Mathf.Abs(block.transform.position.x - transform.position.x), Mathf.Abs(block.transform.position.y - (transform.position.y + transform.localScale.y/2f)));
        if(distVector.magnitude < 2f && held == null)
        {
            setNewHighlightedBlock(block);
        }
    }
}
